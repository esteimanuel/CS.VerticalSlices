using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CS.VerticalSlices.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using FluentValidation;

namespace CS.VerticalSlices.Features.Products
{
    public class Overview
    {
        public class Query : IRequest<Page>
        {
            public string Search { get; set; } = "";
            public int PageIndex { get; set; } = 0;
            public int PageSize { get; set; } = 10;
            public Sorting Sort { get; set; } = Sorting.Code;
            public bool Desc { get; set; } = false;

            public enum Sorting
            {
                Code,
                Description,
            }
            
            public void Validate()
            {
                var validator = new Validator();
                var validationResult = validator.Validate(this);
                if (!validationResult.IsValid) throw new Exception(validationResult.ToString());
            }

            public class Validator : AbstractValidator<Query>
            {
                public Validator()
                {
                    RuleFor(x => x.PageIndex).NotEmpty();
                    RuleFor(x => x.PageSize).GreaterThanOrEqualTo(5);
                }
            }
        }

        public class Page
        {
            public int Index { get; set; }

            public int Size { get; set; }

            public List<Product> Products { get; set; }

            public int ProductsTotal { get; set; }

            public class Product
            {
                public string Code { get; set; }
                public string Description { get; internal set; }
            }
        }

        public class Handler : IRequestHandler<Query, Page>
        {
            private readonly ShopContext context;

            public Handler(ShopContext context) => this.context = context;

            public async Task<Page> Handle(Query request, CancellationToken cancellationToken)
            {
                var filtered = 
                    string.IsNullOrWhiteSpace(request.Search)
                    ? this.context.Products
                    : this.context.Products.Where(product => 
                        product.Code.Contains(request.Search) || 
                        product.Description.Contains(request.Search));

                var sorted = 
                    request.Desc 
                    ? request.Sort switch
                    {
                        Query.Sorting.Code => filtered.OrderByDescending(x => x.Code),
                        Query.Sorting.Description => filtered.OrderByDescending(x => x.Description),
                        _ => throw new ArgumentException(message: "invalid enum value", paramName: nameof(request.Sort))
                    }
                    : request.Sort switch 
                    { 
                        Query.Sorting.Code => filtered.OrderBy(x => x.Code),
                        Query.Sorting.Description => filtered.OrderBy(x => x.Description),
                        _ => throw new ArgumentException(message: "invalid enum value", paramName: nameof(request.Sort))
                    };

                var paged =
                    sorted
                    .Skip(request.PageIndex * request.PageSize)
                    .Take(request.PageSize);

                var products =
                    await paged
                    .Select(product => new Page.Product 
                    {
                        Code = product.Code,
                        Description = product.Description
                    })
                    .ToListAsync();

                var total = filtered.Count();

                return new Page
                {
                    Index = request.PageIndex,
                    Size = request.PageSize,
                    Products = products,
                    ProductsTotal = total
                };
            }
        }
    }
}
