using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace CameraLiveSwitcher
{
    public static class CameraLiveSwitcherUtility
    {
        public static List<Type> CameraPostProductionTypes = GetTypes(typeof(CameraPostProductionBase));
        
        
       
        public static List<Type> GetTypes(Type T)
        {
            var assemblyList = AppDomain.CurrentDomain.GetAssemblies();

            var typeList = new List<Type>();
            foreach ( var assembly in assemblyList )
            {
                
                //
                if ( assembly == null )
                {
                    continue;
                }
                

                var types = assembly.GetTypes();
                typeList.AddRange(types.Where(t => t.IsSubclassOf(T))
                    .ToList());
              
            }

            return typeList;
        }
        
        public static Type GetTypeByClassName( string className )
        {
            foreach( Assembly assembly in AppDomain.CurrentDomain.GetAssemblies() ) {
                foreach( Type type in assembly.GetTypes() ) {
                    if( type.Name == className ) {
                        return type;
                    }
                }
            }
            return null;
        }
    }
}