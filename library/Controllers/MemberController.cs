using library.Data;
using library.Models;
using Microsoft.AspNetCore.Mvc;

namespace library.Controllers;

public class MembersController : Controller
{
    // ─── 13.9 : the field and the constructor ───────────────────────
    private readonly MemberRepository _members;

    public MembersController(MemberRepository members)
    {
        _members = members;
    }


    // GET /Members : the list of members.
    public async Task<IActionResult> Index()
    {
        var members = await _members.GetAllAsync();
        return View(members);
    }
    // GET /Members/Create : an empty form.
    public IActionResult Create()
    {
        return View(new Member());
    }
    [HttPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(Member member)
    {
        if (!ModelState.IsValid)                                     // 1. → D
        {
            return View(member);                                     // 2. → B
        }
        try
        {
            await _members.AddAsync(member);                         // 3. → A
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation) // 4. → F
        {
            ModelState.AddModelError("Email", "Another member already uses this email"); // 5. → C
            return View(member);                                     // 6. → B
        }
        return RedirectToAction(nameof(Index));                      // 7. → D
    }
    // GET /Members/Edit/5 : the form, filled with member 5.
    public async Task<IActionResult> Edit(long id)
    {
        var member = await _members.GetByIdAsync(id);
        if (member == null)
        {
            return NotFound();
        }
        return View(member);
    }

    // POST /Members/Edit/5 : the changed form was submitted.
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(Member member)
    {
        if (!ModelState.IsValid)                                     // 1. → B
        {
            return View(member);                                     // 2. → F
        }
        try
        {
            await _members.UpdateAsync(member);                      // 3. → E
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.UniqueViolation) // 4. → C
        {
            ModelState.AddModelError("Email", "Another member already uses this email"); // 5. → A
            return View(member);
        }
        return RedirectToAction(nameof(Index));                      // 7. → D
    }
    // GET /Members/Delete/5 : ask "are you sure?"
    public async Task<IActionResult> Delete(long id)
    {
        var member = await _members.GetByIdAsync(id);
        if (member == null)
        {
            return NotFound();
        }
        return View(member);
    }

    [HttPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(long id)
    {
        try
        {
            await _members.DeleteAsync(id);                          
        }
        catch (PostgresException ex) when (ex.SqlState == PostgresErrorCodes.ForeignKeyViolation) 
        {
            var member = await _members.GetByIdAsync(id);          
            if (member == null)                                     
            {
                return NotFound();                                 
            }
            ViewBag.Error = "This member has loans on record, so the database will not delete them"; 
            return View("Delete", member);                          
        }
        return RedirectToAction(nameof(Index));                      
    }
}   