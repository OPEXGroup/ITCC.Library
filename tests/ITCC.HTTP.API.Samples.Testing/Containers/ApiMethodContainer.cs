// This is an open source non-commercial project. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++ and C#: http://www.viva64.com

using System.Collections.Generic;
using ITCC.HTTP.API.Attributes;
using ITCC.HTTP.API.Enums;
using ITCC.HTTP.API.Samples.Testing.Utils;
using ITCC.HTTP.API.Samples.Testing.Views;
using ITCC.HTTP.API.Utils;

namespace ITCC.HTTP.API.Samples.Testing.Containers
{
    public static class ApiMethodContainer
    {
        [ApiRequestProcessor(
            description: "Gets data from database",
            subUri: "/items",
            method: ApiHttpMethod.Get,
            authRequired: false)]
        [ApiQueryParam("from", "Initial timestamp", false)]
        [ApiQueryParam("to", "Initial timestamp", false)]
        [ApiRequestBodyType(typeof(Empty))]
        [ApiResponseBodyType(typeof(FirstRootView), typeof(List<FirstRootView>))]
        public static CustomRequestProcessor GetProcessor { get; set; }

        [ApiRequestProcessor(
            description: "Inserts data into database",
            subUri: "/insert",
            method: ApiHttpMethod.Post,
            authRequired: true,
            remarks: "Dont use too much.")]
        [ApiRequestBodyType(typeof(SecondRootView), typeof(List<SecondRootView>))]
        [ApiResponseBodyType(typeof(Empty))]
        public static CustomRequestProcessor SetProcessor { get; set; }
    }
}
