using System;
using System.Collections.Generic;
using System.Text;

namespace SharedProject
{
    public class GUIDGenerator
    {
        /// <summary>
        /// Return a brand new GUID as a String
        /// </summary>
        public static string getGuid()
        {
            var guid = Guid.NewGuid();
            return guid.ToString();
        }
    }
}
