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

            return new CreateParameterValueResult { 
                Value = value, IsCreated = true
            };
        }

        public static CreateParameterValueResult Fail()
        {
            return failResult; 
        }


        // we have some singleton values here to prevent alloc/free memory

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
    }
}
