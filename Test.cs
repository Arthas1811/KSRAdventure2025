using System;
using Newtonsoft.Json.Linq;
class Program {
    static void Main() {
        JArray arr = new JArray();
        arr.Add("kantiCard");
        Console.WriteLine(arr.Contains("kantiCard"));
        arr.Remove("kantiCard");
        Console.WriteLine(arr.Count);
    }
}
