using System.IO;
using System;
using Microsoft.Data.Sqlite;
using Microsoft.Data;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
namespace Sync {
  public class Syncer {
    private DateTime last_sync_;
    private readonly string SYNC_FILE = "./sync_time";
    public Syncer() {
      GetLastSyncTime();
      RunVSDownload();
      WalkVS("./Layout");
      SetLastSyncTime();
    } 

    private void RunVSDownload() {
      Process.Start("vs.exe --layout Layout --all");
    }

    private void SetLastSyncTime() {
      File.WriteAllText(SYNC_FILE, DateTime.UtcNow.ToString());
    }

    private void GetLastSyncTime() {
      if (!File.Exists(SYNC_FILE)) {
        last_sync_  = DateTime.UtcNow;
      } else {
        string date_str = File.ReadAllText(SYNC_FILE);
        last_sync_ = DateTime.Parse(date_str);
      }
    }

    private void WalkVS(string root_dir) {
      DirectoryInfo root = new DirectoryInfo(root_dir);
      FileInfo[] files = root.GetFiles("*.*", SearchOption.AllDirectories);
      foreach(var file in files) {
        if (file.LastWriteTimeUtc > last_sync_) {
          Console.WriteLine(file.FullName);
        }
      }
    }
  }
}