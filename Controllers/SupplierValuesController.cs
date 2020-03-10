using Microsoft.AspNetCore.Mvc;
using ServerApp.Models;
using ServerApp.Models.BindingTargets;
using System.Collections.Generic;
using Microsoft.AspNetCore.Authorization;

namespace ServerApp.Controllers
{
    [Route("api/suppliers")]
    [Authorize(Roles = "Administrator")]
    public class SupplierValuesController : Controller
    {
        private DataContext context;

        public SupplierValuesController(DataContext ctx)
        {
            context = ctx;
        }

        [HttpGet]
        public IEnumerable<Supplier> GetSuppliers()
        {
            return context.Suppliers;
        }

        [HttpPost]
        public IActionResult CreateSupplier([FromBody] SupplierData sData)
        {
            if (ModelState.IsValid)
            {
                var s = sData.Supplier;
                context.Add(s);
                context.SaveChanges();
                return Ok(s.SupplierId);
            }
            else
            {
                return BadRequest(ModelState);
            }
        }
    }

}