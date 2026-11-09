using library.Data;
using library.Models;
using Microsoft.AspNetCore.Mvc;
using Npgsql; // Required for PostgresException and PostgresErrorCodes (Task 3 & 4)
using System;
using System.Threading.Tasks;

namespace library.Controllers
{
    public class MembersController : Controller
    {
        private readonly MemberRepository _members;
        private readonly LoanRepository _loans; // Added in Task 10

        // Constructor: ASP.NET Core injects both repositories.
        public MembersController(MemberRepository members, LoanRepository loans)
        {
            _members = members;
            _loans = loans;
        }

        // GET: /Members
        public async Task<IActionResult> Index()
        {
            var members = await _members.GetAllAsync();
            return View(members);
        }

        // GET: /Members/Details/5
        // OUTLINE 10.4: One member and their loans.
        public async Task<IActionResult> Details(long id)
        {
            var member = await _members.GetByIdAsync(id);   // 1 → E
            if (member == null)                             // 2 → B
            {
                return NotFound();                          // 3 → D
            }

            var details = new MemberDetails                 // 4 → A
            {
                Member = member,
                Loans = await _loans.GetByMemberAsync(id)
            };

            return View(details);                           // 5 → C
        }

        // GET: /Members/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: /Members/Create
        // OUTLINE 4.1: Catch duplicate email on insert.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Member member)
        {
            if (!ModelState.IsValid)                        // 1 → D
            {
                return View(member);                        // 2 → B
            }

            try
            {
                await _members.AddAsync(member);            // 3 → A
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation) // 4 → F
            {
                ModelState.AddModelError("Email", "Another member already uses this email");      // 5 → C
                return View(member);                        // 6 → B
            }

            return RedirectToAction(nameof(Index));         // 7 → D
        }

        // GET: /Members/Edit/5
        public async Task<IActionResult> Edit(long id)
        {
            var member = await _members.GetByIdAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            return View(member);
        }

        // POST: /Members/Edit/5
        // OUTLINE 4.2: Catch duplicate email on update.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Member member)
        {
            if (!ModelState.IsValid)                        // 1 → B
            {
                return View(member);                        // 2 → F
            }

            try
            {
                await _members.UpdateAsync(member);         // 3 → E
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation) // 4 → C
            {
                ModelState.AddModelError("Email", "Another member already uses this email");      // 5 → A
                return View(member);                        // 6 → F
            }

            return RedirectToAction(nameof(Index));         // 7 → D
        }

        // GET: /Members/Delete/5
        public async Task<IActionResult> Delete(long id)
        {
            var member = await _members.GetByIdAsync(id);
            if (member == null)
            {
                return NotFound();
            }
            return View(member);
        }

        // POST: /Members/Delete/5
        // OUTLINE 3.2: Catch foreign key violation (member has loans).
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(long id)
        {
            try
            {
                await _members.DeleteAsync(id);             // 1 → A
            }
            catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation) // 2 → G
            {
                var member = await _members.GetByIdAsync(id); // 3 → B
                if (member == null)                           // 4 → E
                {
                    return NotFound();                        // 5 → C
                }

                ViewBag.Error = "This member has loans on record, so the database will not delete them"; // 6 → H
                return View("Delete", member);                // 7 → F
            }

            return RedirectToAction(nameof(Index));           // 8 → D
        }
    }
}