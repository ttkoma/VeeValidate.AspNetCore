﻿using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace VeeValidate.AspNetCore.Adapters
{
    public class CompareClientValidator : VeeAttributeClientValidator<CompareAttribute>
    {
        public CompareClientValidator(CompareAttribute attribute) : base(attribute)
        {
        }

        public override void AddValidationRules(ClientModelValidationContext context)
        {
            MergeRule(context.Attributes, $"confirmed:{Attribute.OtherProperty}");
        }
    }
}