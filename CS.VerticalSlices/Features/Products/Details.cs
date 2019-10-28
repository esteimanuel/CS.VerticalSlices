using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CS.VerticalSlices.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using FluentValidation;
using System.Collections.Generic;

namespace CS.VerticalSlices.Features.Products
{
    public class Details
    {
        public class Query : IRequest<Model>
        {
            public string Code { get; set; }
            public string Description { get; set; }

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
                    RuleFor(x => x.Code).NotEmpty();
                    RuleFor(x => x.Description).NotEmpty();
                }
            }
        }

        public class Model
        {
            public string Code { get; internal set; }
            public string Description { get; internal set; }
        }

        public class Handler : IRequestHandler<Query, Model>
        {
            private readonly ShopContext context;

            public Handler(ShopContext context) => this.context = context;

            public async Task<Model> Handle(Query request, CancellationToken cancellationToken)
            {
                request.Validate();

                var model =
                    await this.context.Products
                    .Where(product => product.Code == request.Code)
                    .Select(product => new Model 
                    {
                        Code = product.Code,
                        Description = product.Description,
                    })
                    .SingleOrDefaultAsync();

                return model;
            }
        }
    }
}
