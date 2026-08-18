using SAXServices.Contracts;
using SAXServices.DAL;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SAXServices.BL
{
    public class ProductHandler : IHandlerBase<Product>
    {
        ICRUDDAL<Product> _productDAL;

        public ProductHandler():this(new ProductDAL()){ }

        public ProductHandler(ICRUDDAL<Product> dal)
        {
            this._productDAL = dal;
        }

        public Product GetByID(int id)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Product> GetAll()
        {
            return this._productDAL.Get();
        }

        public Product GetByName(string name)
        {
            return this._productDAL.GetByName(name);
        }

        public IEnumerable<Product> GetByDate(DateTime fecha)
        {
            throw new NotImplementedException();
        }

        public bool Save(Product order, out string message)
        {
            throw new NotImplementedException();
        }


        //private Product MockProduct(int i, bool hasVariations)
        //{
        //    var variations = new List<ProductVariation>();

        //    if (hasVariations)
        //    {
        //        for (int j = 0; j < i; j++)
        //        {
        //            var subVariations = new List<ProductVariation>();

        //            for (int k = 0; k < j; k++)
        //            {
        //                subVariations.Add(new ProductVariation { AttributeName = "Talle", AttributeValue = "Talle" + k, Description = "Talle" + k, Manage_Stock = true, ParentProduct_Id = "ART" + i, Stock = 10, Variations = null });
        //            }

        //            variations.Add(new ProductVariation { AttributeName = "Color", AttributeValue = "Color" + j, Description = "Color" + j, Manage_Stock = false, ParentProduct_Id = "ART" + i, Stock = 0, Variations = subVariations });
        //        }
        //    }

        //    var producto = new Product { Description = "Artículo " + i, Product_Id = "ART" + i, Name = "ART " + i, Manage_Stock = !hasVariations, Type = ProductType.simple, Stock = 10, LastUpdate = DateTime.Now, Variations = variations };

        //    return producto;
        //}
    }
}
