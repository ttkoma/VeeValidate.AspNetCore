﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Internal;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Options;

namespace VeeValidate.AspNetCore.ViewFeatures
{
    public class VeeValidateHtmlAttributeProvider : DefaultValidationHtmlAttributeProvider
    {
        private readonly VeeValidateOptions _options;
        private readonly IDictionary<string, Func<string, ModelMetadata, string>> _adapters = new ConcurrentDictionary<string, Func<string, ModelMetadata, string>>();

        public VeeValidateHtmlAttributeProvider(
            IOptions<MvcViewOptions> optionsAccessor, 
            IModelMetadataProvider metadataProvider, 
            ClientValidatorCache clientValidatorCache, 
            VeeValidateOptions options,
            IEnumerable<IHtmlValidationAttributeAdapter> adapters) : base(optionsAccessor, metadataProvider, clientValidatorCache)
        {
            _options = options;

            // Convert the list of adapters into a dictionary of keys and functions.
            foreach (var adapter in adapters)
            {
                foreach (var key in adapter.Keys)
                {
                    // Only add one handler per key.
                    if (!_adapters.ContainsKey(key))
                    {
                        _adapters.Add(key, adapter.GetVeeValidateRule);
                    }
                }
            }
        }

        public override void AddValidationAttributes(ViewContext viewContext, ModelExplorer modelExplorer, IDictionary<string, string> attributes)
        {
            base.AddValidationAttributes(viewContext, modelExplorer, attributes);
            
            if (attributes.TryGetValue("data-val", out string validate))
            {
                if (validate == "true")
                {
                    AddVeeValidateAttribute(modelExplorer, attributes);

                    if (!attributes.ContainsKey("data-vv-as"))
                    {
                        // Set data-vv-as attribute to give clean error description.
                        attributes["data-vv-as"] = modelExplorer.Metadata.GetDisplayName();
                    }

                    // Add a class binding to toggle the input error class when the field is in an invalid state.
                    VueHtmlAttributeHelper.MergeBindAttribute(
                        attributes, 
                        "class", 
                        $"{{'{_options.ValidationInputCssClassName}': {_options.ErrorBagName}.has('{(attributes.ContainsKey("data-vv-name") ? attributes["data-vv-name"] : attributes["name"])}') }}");
                }
            }
        }

        private void AddVeeValidateAttribute(ModelExplorer modelExplorer, IDictionary<string, string> attributes)
        {
            ICollection<string> rules = new Collection<string>();
            
            // Convert the default validation attributes to vee validate attributes
            var validationAttributes = attributes.Where(x => x.Key.StartsWith("data-val"));
            foreach (var attribute in validationAttributes)
            {
                if (_adapters.TryGetValue(attribute.Key, out var handler))
                {
                    rules.Add(handler(attribute.Value, modelExplorer.Metadata));
                }

                // Remove the default validation attribute
                attributes.Remove(attribute);
            }

            // Get any existing rules declared in the markup.
            if (attributes.TryGetValue("v-validate", out var existingRules))
            {
                if (existingRules.StartsWith("'"))
                {
                    throw new Exception("v-validate attributes must be declared in object format.");
                }

                existingRules = existingRules.TrimStart('{').TrimEnd('}');
            }
            
            // TODO - Merge existing rules with rules

            // Use the object format for the rules.
            attributes["v-validate"] = "{" + string.Join(",", rules) + "}";
        }
    }
}