using System;

namespace VS {
  class Program {
    static void Main(string[] args) {
        Console.WriteLine("Running Visual Studio sync tool!");
        Sync.Syncer s = new Sync.Syncer();
    }
  }
}
