// /*
//     Copyright (C) 2020  erri120
// 
//     This program is free software: you can redistribute it and/or modify
//     it under the terms of the GNU General Public License as published by
//     the Free Software Foundation, either version 3 of the License, or
//     (at your option) any later version.
// 
//     This program is distributed in the hope that it will be useful,
//     but WITHOUT ANY WARRANTY; without even the implied warranty of
//     MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
//     GNU General Public License for more details.
// 
//     You should have received a copy of the GNU General Public License
//     along with this program.  If not, see <https://www.gnu.org/licenses/>.
// */

using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;

namespace SilentMoonsEnchantmentPatcher
{
    public static class JsonUtils
    {
        public static T FromJson<T>(string file)
        {
            if (!File.Exists(file))
                throw new ArgumentException($"File {file} does not exist!");
            
            var serializer = new JsonSerializer
            {
                NullValueHandling = NullValueHandling.Include,
            };

            using var sr = new StreamReader(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read), Encoding.UTF8);
            var jsonReader = new JsonTextReader(sr);
            return serializer.Deserialize<T>(jsonReader)!;
        }
    }
}
