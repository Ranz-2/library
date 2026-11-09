using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using library.Data;
using library.Models;

namespace library.Controllers
{
    public class LoansController : Controller
    {
        private readonly LoanRepository _loans;
        private readonly MemberRepository _members;
        private readonly BookRepository _books;

        public LoansController(LoanRepository loans, MemberRepository members, BookRepository books)
        {
            _loans = loans;
            _members = members;
            _books = books;
        }

        // GET /Loans
        public async Task<IActionResult> Index()
        {
            var loans = await _loans.GetAllAsync();
            return View(loans);
        }

        // SNIPPET 8.3 — fill both dropdowns
        private async Task FillDropdownsAsync()
        {
            ViewBag.Members = new SelectList(await _members.GetAllAsync(), "MemberId", "FullName");
            ViewBag.Books = new SelectList(await _books.GetAllAsync(), "BookId", "Title");
        }

        // SNIPPET 8.4 — GET /Loans/Borrow
        public async Task<IActionResult> Borrow()
        {
            await FillDropdownsAsync();
            return View(new BorrowForm());
        }

        // OUTLINE 8.5 — POST /Loans/Borrow
        [HttPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Borrow(BorrowForm form)
        {
            if (ModelState.IsValid)                                                  // 1 → E
            {
                bool saved = await _loans.BorrowAsync(                               // 2 → B
                    form.MemberId!.Value, form.BookId!.Value, form.DaysAllowed);

                if (saved)                                                           // 3 → A
                {
                    return RedirectToAction(nameof(Index));                          // 4 → G
                }
                ModelState.AddModelError("",
                    $"This member already has {LoanRepository.MaxOpenLoans} books out. Nothing was saved."); // 5 → F
            }

            await FillDropdownsAsync();                                              // 6 → D
            return View(form);                                                       // 7 → C
        }
    }
    // POST /Loans/Return/5 : the Return button was pressed.
[HttPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Return(long id)
        {
            await _loans.ReturnAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }