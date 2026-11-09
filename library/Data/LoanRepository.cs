using library.Models;
using Npgsql;

namespace library.Data;

public class LoanRepository
{
    public const int MaxOpenLoans = 3;

    private readonly NpgsqlDataSource _dataSource;

    public LoanRepository(NpgsqlDataSource dataSource) => _dataSource = dataSource;

    // 7.3 ReadLoan
    private static LoanView ReadLoan(NpgsqlDataReader reader)
    {
        return new LoanView
        {
            LoanId = reader.GetInt64(0),                                   // 1 → D
            MemberId = reader.GetInt64(1),                                   // 2 → F
            MemberName = reader.GetString(2),                                  // 3 → H
            BookTitle = reader.GetString(3),                                  // 4 → B
            LoanedOn = reader.GetFieldValue<DateOnly>(4).ToDateTime(TimeOnly.MinValue), // 5 → E
            ReturnedOn = reader.IsDBNull(5)
                            ? null
                            : reader.GetFieldValue<DateOnly>(5).ToDateTime(TimeOnly.MinValue), // 6 → C
            DaysAllowed = reader.GetInt32(6),                                  // 7 → A
            Status = reader.GetString(7)                                  // 8 → G
        };
    }

    // 7.4 GetAllAsync
    public async Task<List<LoanView>> GetAllAsync()
    {
        const string sql =                                                   // 1
            "SELECT l.loan_id, l.member_id, m.full_name, b.title, " +
            "       l.loaned_on, l.returned_on, l.days_allowed, " +
            "       CASE WHEN l.returned_on IS NOT NULL THEN 'Returned' " +
            "            WHEN l.loaned_on + l.days_allowed < CURRENT_DATE THEN 'Overdue' " +
            "            ELSE 'Out' END " +
            "FROM lending.loan l " +
            "JOIN lending.member m ON l.member_id = m.member_id " +
            "JOIN lending.book   b ON l.book_id   = b.book_id " +
            "ORDER BY l.loaned_on DESC, l.loan_id DESC;";

        var loans = new List<LoanView>();                                    // 2
        await using var command = _dataSource.CreateCommand(sql);            // 3
        await using var reader = await command.ExecuteReaderAsync();         // 4

        while (await reader.ReadAsync())                                     // 5
        {
            loans.Add(ReadLoan(reader));                                     // 6
        }
        return loans;                                                        // 7
    }
    // BORROW: lend one book, unless the member already has MaxOpenLoans books out.
    // The check and the INSERT run inside one transaction: both happen, or neither does.
    public async Task<bool> BorrowAsync(long memberId, long bookId, int daysAllowed)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();          // 1 → A
        await using var transaction = await connection.BeginTransactionAsync();        // 2 → G

        // 3 → D: lock the member's row so a second borrow for the same member waits here.
        await using (var lockMember = new NpgsqlCommand(
            "SELECT member_id FROM lending.member WHERE member_id = @member_id FOR UPDATE;",
            connection, transaction))
        {
            lockMember.Parameters.AddWithValue("member_id", memberId);
            await lockMember.ExecuteNonQueryAsync();
        }

        // 4: count how many books this member has out right now.
        long openLoans;
        await using (var count = new NpgsqlCommand(
            "SELECT COUNT(*) FROM lending.loan WHERE member_id = @member_id AND returned_on IS NULL;",
            connection, transaction))
        {
            count.Parameters.AddWithValue("member_id", memberId);
            openLoans = Convert.ToInt64(await count.ExecuteScalarAsync());
        }

        if (openLoans >= MaxOpenLoans)                                                 // 5 → E
        {
            await transaction.RollbackAsync();                                         // 6 → I
            return false;                                                              // 7 → B
        }

        // 8 → C: record the loan. loaned_on is filled by the database.
        await using (var insert = new NpgsqlCommand(
            "INSERT INTO lending.loan (member_id, book_id, days_allowed) " +
            "VALUES (@member_id, @book_id, @days_allowed);",
            connection, transaction))
        {
            insert.Parameters.AddWithValue("member_id", memberId);
            insert.Parameters.AddWithValue("book_id", bookId);
            insert.Parameters.AddWithValue("days_allowed", daysAllowed);
            await insert.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();                                               // 9 → F
        return true;                                                                   // 10 → H
    }
    // RETURN: mark a loan as returned today. A loan already returned is left alone.
    public async Task ReturnAsync(long loanId)
    {
        const string sql =                                                       // 1 → C
            "UPDATE lending.loan SET returned_on = CURRENT_DATE " +
            "WHERE loan_id = @id AND returned_on IS NULL;";

        await using var command = _dataSource.CreateCommand(sql);                // 2 → A
        command.Parameters.AddWithValue("id", loanId);                           // 3 → D
        await command.ExecuteNonQueryAsync();                                    // 4 → B
    }
}