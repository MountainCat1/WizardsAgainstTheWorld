using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json.Serialization;

namespace Data
{
    public class CustomSerializationBinder : DefaultSerializationBinder
    {
        private static readonly HashSet<Type> TypesToShorten = new()
        {
            typeof(WeaponValueModifier),
            typeof(WeaponSpecialModifier),
        };

        public override Type BindToType(string assemblyName, string typeName)
        {
            // If the JSON only contains a class name, find the matching type
            Type matchedType = TypesToShorten.FirstOrDefault(t => t.Name == typeName);
            return matchedType ?? base.BindToType(assemblyName, typeName);
        }

        public override void BindToName(Type serializedType, out string assemblyName, out string typeName)
        {
            if (TypesToShorten.Contains(serializedType))
            {
                assemblyName = null; // No assembly needed
                typeName = serializedType.Name; // Use only the class name
            }
            else
            {
                base.BindToName(serializedType, out assemblyName, out typeName);
            }
        }
    }
}