using PrimaryDLL;
using System;

namespace DependentDLL
{
    public class DependentDLLClass
    {
        public string GetStringFromPrimaryDLL()
        {
            PrimaryDLLClass primaryDLL = new PrimaryDLLClass();
            return primaryDLL.GetPrimaryString();
        }
    }
}
