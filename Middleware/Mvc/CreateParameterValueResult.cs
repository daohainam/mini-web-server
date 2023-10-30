using Microsoft.AspNetCore.Http;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MiniWebServer.Mvc
{
    internal class CreateParameterValueResult
    {
        public required object? Value { get; internal init; }
        public required bool IsCreated { get; internal init; }

        public static CreateParameterValueResult Success(object? value)
        {
            if (value == null)
            {
                return successNullResult;
            }
            else if (value is string v && v.Length == 0) // if this is an empty string, we use the cached result to prevent memory allocation
            {
                return successEmptyResult;
            }

            return new CreateParameterValueResult { 
                Value = value, IsCreated = true
            };
        }

        public static CreateParameterValueResult Fail()
        {
            return failResult; 
        }


        // we have some singleton values here to prevent alloc/free memory blocks
        private static readonly CreateParameterValueResult failResult = new() 
        {
            Value = null,
            IsCreated = false
        };

        private static readonly CreateParameterValueResult successNullResult = new()
        {
            Value = null,
            IsCreated = true
        };

        private static readonly CreateParameterValueResult successEmptyResult = new()
        {
            Value = string.Empty,
            IsCreated = true
        };
    }
}
