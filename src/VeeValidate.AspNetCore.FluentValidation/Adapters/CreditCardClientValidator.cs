﻿using FluentValidation.AspNetCore;
using FluentValidation.Internal;
using FluentValidation.Validators;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VeeValidate.AspNetCore.FluentValidation.Adapters
{
    public class CreditCardClientValidator : ClientValidatorBase
    {
        public CreditCardClientValidator(PropertyRule rule, IPropertyValidator validator)
            : base(rule, validator)
        {
        }

        public override void AddValidation(ClientModelValidationContext context)
        {
            context.AddValidationRule("credit_card", "true");
        }
    }
}
