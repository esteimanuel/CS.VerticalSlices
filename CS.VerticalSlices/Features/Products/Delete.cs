using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CS.VerticalSlices.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System;
using FluentValidation;

namespace CS.VerticalSlices.Features.Products
{
    public class Delete
    {
        public class Query : IRequest<string>
        {
            public string Code { get; set; }

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
                }
            }
        }
        
        public class Handler : IRequestHandler<Query, string>
        {
            private readonly ShopContext context;

            public Handler(ShopContext context) => this.context = context;

            public async Task<string> Handle(Query request, CancellationToken cancellationToken)
            {
                request.Validate();

                var model =
                    await this.context.Products
                    .Where(product => product.Code == request.Code)
                    .SingleAsync();

                this.context.Remove(model);
                this.context.SaveChanges();

                return request.Code;
            }
        }
    }
}
