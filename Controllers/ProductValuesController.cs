using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.EntityFrameworkCore;
using ServerApp.Models;
using ServerApp.Models.BindingTargets;

namespace ServerApp.Controllers
{
    [Route("api/products")]
    [ApiController]
    public class ProductValuesController : Controller
    {
        private readonly DataContext context;

        public ProductValuesController(DataContext ctx)
        {
            context = ctx;
        }
        [HttpGet("{id}")]
        public Product GetProduct(long id)
        {
            //System.Threading.Thread.Sleep(5000);
            var result = context.Products
                .Include(p => p.Supplier).ThenInclude(s => s.Products)
                .Include(p => p.Ratings)
                .FirstOrDefault(p => p.ProductId == id);

            if (result != null)
            {
                if (result.Supplier != null)
                {
                    result.Supplier.Products = result.Supplier.Products
                        .Select(p => new Product
                        {
                            ProductId = p.ProductId,
                            Name = p.Name,
                            Category = p.Category,
                            Description = p.Description,
                            Price = p.Price,
                        });
                }
                if (result.Ratings != null)
                {
                    foreach (Rating r in result.Ratings)
                    {
                        r.Product = null;
                    }
                }
            }

            return result;
        }
        [HttpGet]
        public IActionResult GetProducts(string category, string search, bool related = false, bool metadata = false)
        {
            IQueryable<Product> query = context.Products;
            if (!string.IsNullOrWhiteSpace(category))
            {
                string catLower = category.ToLower();
                query = query.Where(p => p.Category.ToLower().Contains(catLower));
            }
            if (!string.IsNullOrWhiteSpace(search))
            {
                string searchLower = search.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(searchLower)
                                         || p.Description.ToLower().Contains(searchLower));
            }
            if (related)
            {
                query = query.Include(p => p.Supplier).Include(p => p.Ratings);
                List<Product> data = query.ToList();
                data.ForEach(p =>
                {
                    if (p.Supplier != null)
                        p.Supplier.Products = null;

                    p.Ratings?.ForEach(r => r.Product = null);
                });
                return metadata ? CreateMetadata(data) : Ok(data);
            }
            return metadata ? CreateMetadata(query) : Ok(query);
        }
        private IActionResult CreateMetadata(IEnumerable<Product> products)
        {
            return Ok(new
            {
                data = products,
                categories = context.Products.Select(p => p.Category)
                    .Distinct().OrderBy(c => c)
            });
        }
        // [HttpGet]
        // public IEnumerable<Product> GetProducts(bool related = false)
        // {
        //     IQueryable<Product> query = context.Products;
        //     if (related)
        //     {
        //         query = query.Include(p => p.Supplier)
        //             .Include(p => p.Ratings);

        //         var data = query.ToList();
        //         data.ForEach(p =>
        //         {
        //             if (p.Supplier != null)
        //                 p.Supplier.Products = null;

        //             p.Ratings?.ForEach(r => r.Product = null);
        //         });
        //         return data;
        //     }
        //     return query;
        // }
        [HttpPost]
        public IActionResult CreateProduct([FromBody] ProductData pdata)
        {
            if (ModelState.IsValid)
            {
                Product p = pdata.Product;
                if (p.Supplier != null && p.Supplier.SupplierId != 0)
                {
                    context.Attach(p.Supplier);
                }
                context.Add(p);
                context.SaveChanges();
                return Ok(p.ProductId);
            }
            return BadRequest(ModelState);
        }

        [HttpPut("{id}")]
        public IActionResult ReplaceProduct(long id, [FromBody] ProductData pData)
        {
            if (ModelState.IsValid)
            {
                var p = pData.Product;
                p.ProductId = id;
                if (p.Supplier != null && p.Supplier.SupplierId != 0)
                {
                    context.Attach(p.Supplier);
                }
                context.Update(p);
                context.SaveChanges();
                return Ok();
            }
            return BadRequest(ModelState);
        }

        public IActionResult UpdateProduct(long id, [FromBody] JsonPatchDocument<ProductData> patch)
        {
            var product = context.Products.Include(p => p.Supplier)
                .First(p => p.ProductId == id);

            var data = new ProductData() { Product = product };
            patch.ApplyTo(data, ModelState);

            if (ModelState.IsValid && TryValidateModel(data))
            {
                if (product.Supplier != null && product.Supplier.SupplierId != 0)
                    context.Attach(product.Supplier);
                context.SaveChanges();
                return Ok();
            }

            return BadRequest(ModelState);
        }
        [HttpDelete("{id}")]
        public void DeleteProduct(long id)
        {
            context.Remove(new Product() { ProductId = id });
            context.SaveChanges();
        }
        [HttpDelete("{id}")]
        public void DeleteSupplier(long id)
        {
            context.Remove(new Supplier() { SupplierId = id });
            context.SaveChanges();
        }
    }
}
