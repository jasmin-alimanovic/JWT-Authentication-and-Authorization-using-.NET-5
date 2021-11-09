using JWTAuthentication.Data;
using JWTAuthentication.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;

namespace JWTAuthentication.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme, Roles = "AppUser")]
    public class TodosController : ControllerBase
    {
        private readonly AppDbContext _context;

        public TodosController(AppDbContext context)
        {
            _context = context;
        }


        [HttpGet]
        [Authorize(Policy = "DepartmentPolicy")]
        public async Task<IActionResult> GetTodosAsync()
        {
            var todos = await _context.Todos.ToListAsync();
            return Ok(todos);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetTodo(int id)
        {
            var todo = await _context.Todos.FirstOrDefaultAsync(t => t.Id == id);

            if(todo == null)
            {
                return NotFound();
            }
            return Ok(todo);
        }


        [HttpPost]
        public async Task<IActionResult> CreateTodo(Todo todo)
        {
            if(ModelState.IsValid)
            {
                await _context.Todos.AddAsync(todo);
                await _context.SaveChangesAsync();

                return CreatedAtAction("GetTodo", new { todo.Id }, todo);
            }
            return new JsonResult("Something went wrong") { StatusCode = 500 };
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTodo(int id, Todo item)
        {
            if (id != item.Id)
                return BadRequest();

            var existItem = await _context.Todos.FirstOrDefaultAsync(x => x.Id == id);

            if (existItem == null)
                return NotFound();

            existItem.Title = item.Title;
            existItem.Description = item.Description;
            existItem.Done = item.Done;

            // Implement the changes on the database level
            await _context.SaveChangesAsync();

            return NoContent();
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteItem(int id)
        {
            var existItem = await _context.Todos.FirstOrDefaultAsync(x => x.Id == id);

            if (existItem == null)
                return NotFound();

            _context.Todos.Remove(existItem);
            await _context.SaveChangesAsync();

            return Ok(existItem);
        }

    }
}
