using library.Models;
using Npgsql;

namespace library.Data;

public class LoanRepository
{
    public const int MaxOpenLoans = 3;
    private readonly NpgsqlDataSource _dataSource;

    public LoanRepository(NpgsqlDataSource dataSource)
    {
        _dataSource = dataSource;
    }

    private static LoanView ReadLoan(NpgsqlDataReader reader)
    {
        return new LoanView
        {
            LoanId = reader.GetInt64(0),
            MemberId = reader.GetInt64(1),
            MemberName = reader.GetString(2),
            BookTitle = reader.GetString(3),
            LoanedOn = reader.GetFieldValue<DateOnly>(4).ToDateTime(TimeOnly.MinValue),
            ReturnedOn = reader.IsDBNull(5) ? null : reader.GetFieldValue<DateOnly>(5).ToDateTime(TimeOnly.MinValue),
            DaysAllowed = reader.GetInt32(6),
            Status = reader.GetString(7)
        };
    }

    // READ: every loan with its member and book, newest first.
    public async Task<List<LoanView>> GetAllAsync()
    {
        const string sql =
            "SELECT l.loan_id, l.member_id, m.full_name, b.title, l.loaned_on, l.returned_on, l.days_allowed, " +
            "CASE WHEN l.returned_on IS NOT NULL THEN 'Returned' " +
            "WHEN l.loaned_on + l.days_allowed < CURRENT_DATE THEN 'Overdue' " +
            "ELSE 'Out' END " +
            "FROM lending.loan l " +
            "JOIN lending.member m ON l.member_id = m.member_id " +
            "JOIN lending.book b ON l.book_id = b.book_id " +
            "ORDER BY l.loaned_on DESC, l.loan_id DESC;";

        var loans = new List<LoanView>();
        await using var command = _dataSource.CreateCommand(sql);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            loans.Add(ReadLoan(reader));
        }
        return loans;
    }

    // BORROW: lock the member, count their open loans, and insert only if under the limit.
    public async Task<bool> BorrowAsync(long memberId, long bookId, int daysAllowed)
    {
        await using var connection = await _dataSource.OpenConnectionAsync();
        await using var transaction = await connection.BeginTransactionAsync();

        // 3. Lock this member's row
        await using (var lockMember = new NpgsqlCommand(
            "SELECT member_id FROM lending.member WHERE member_id = @member_id FOR UPDATE;", connection, transaction))
        {
            lockMember.Parameters.AddWithValue("member_id", memberId);
            await lockMember.ExecuteNonQueryAsync();
        }

        // 4. Count the books this member has out.
        long openLoans;
        await using (var count = new NpgsqlCommand(
            "SELECT COUNT(*) FROM lending.loan WHERE member_id = @member_id AND returned_on IS NULL;", connection, transaction))
        {
            count.Parameters.AddWithValue("member_id", memberId);
            openLoans = Convert.ToInt64(await count.ExecuteScalarAsync());
        }

        // 5. Are they at the limit?
        if (openLoans >= MaxOpenLoans)
        {
            await transaction.RollbackAsync();
            return false;
        }

        // 8. Record the loan.
        await using (var insert = new NpgsqlCommand(
            "INSERT INTO lending.loan (member_id, book_id, days_allowed) VALUES (@member_id, @book_id, @days_allowed);",
            connection, transaction))
        {
            insert.Parameters.AddWithValue("member_id", memberId);
            insert.Parameters.AddWithValue("book_id", bookId);
            insert.Parameters.AddWithValue("days_allowed", daysAllowed);
            await insert.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();
        return true;
    }

    // RETURN: mark a loan as returned today.
    public async Task ReturnAsync(long loanId)
    {
        const string sql = "UPDATE lending.loan SET returned_on = CURRENT_DATE WHERE loan_id = @id AND returned_on IS NULL;";
        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("id", loanId);
        await command.ExecuteNonQueryAsync();
    }

    // READ: one member's loans
    public async Task<List<LoanView>> GetByMemberAsync(long memberId)
    {
        const string sql =
            "SELECT l.loan_id, l.member_id, m.full_name, b.title, l.loaned_on, l.returned_on, l.days_allowed, " +
            "CASE WHEN l.returned_on IS NOT NULL THEN 'Returned' " +
            "WHEN l.loaned_on + l.days_allowed < CURRENT_DATE THEN 'Overdue' " +
            "ELSE 'Out' END " +
            "FROM lending.loan l " +
            "JOIN lending.member m ON l.member_id = m.member_id " +
            "JOIN lending.book b ON l.book_id = b.book_id " +
            "WHERE l.member_id = @member_id " +
            "ORDER BY l.loaned_on DESC, l.loan_id DESC;";

        var loans = new List<LoanView>();
        await using var command = _dataSource.CreateCommand(sql);
        command.Parameters.AddWithValue("member_id", memberId);
        await using var reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            loans.Add(ReadLoan(reader));
        }
        return loans;
    }
}