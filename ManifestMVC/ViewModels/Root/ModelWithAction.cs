using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace ManifestMVC.ViewModels.Root
{
    public class ModelWithAction<T>
    {
        public T VM { get; set; }

        public string Action { get; set; }
    }


    public static class ModelWithAction
    {
        public static ModelWithAction<T> Create<T>(string act, T vm)
        {
            return new ModelWithAction<T>()
            {
                VM = vm,
                Action = act
            };
        }
    }

}