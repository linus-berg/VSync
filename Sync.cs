using System;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace Sync {
  public class Syncer {
    private DateTime last_sync_;
    private bool basing_ = false;
    private readonly string SYNC_FILE = "./sync_time";
    private readonly string OUT_DIR = "./OUT/";
    private readonly string ZIP_DIR = "./ZIP/";
    private string args_ = File.ReadAllText("ARGS.txt");
    private string layout_dir_ = null;
    public Syncer() {
      GetLastSyncTime();
      GetLayoutdir();
      ClearOut();
      RunVSDownload();
      if (!basing_) {
        WalkVS("./Layout");
      }
      SetLastSyncTime();
      Zip();
    } 
    private void GetLayoutdir() {
      string[] a = args_.Split(" ");
      for(int i = 0; i < a.Length; i++) {
        if (a[i] == "--layout") {
          layout_dir_ = a[i+1];
          break;
        }
      }
    }
    private void Zip() {
      Console.WriteLine("Zipping Delta Files.");
      if (!Directory.Exists(ZIP_DIR)) {
        Directory.CreateDirectory(ZIP_DIR);
      }
      ZipFile.CreateFromDirectory(OUT_DIR, ZIP_DIR + $"{last_sync_.ToString("dd_MM_yyyy_hhmmss")}.zip");
      DeleteOUT();
    }

    private void DeleteOUT() {
      if (Directory.Exists(OUT_DIR)) {
        Directory.Delete(OUT_DIR, true);
      }
    }
    private void ClearOut() {
      DeleteOUT();
      Directory.CreateDirectory(OUT_DIR);
    }

    private void RunVSDownload() {
      Console.WriteLine("Starting Visual Studio Download / Syncing");
      var p = Process.Start("./vs.exe", args_);
      p.WaitForExit(); 
    }

    private void SetLastSyncTime() {
      last_sync_ = DateTime.UtcNow;
      File.WriteAllText(SYNC_FILE, last_sync_.ToString());
      Console.WriteLine($"Setting last sync time: {last_sync_}.");
    }

    private void GetLastSyncTime() {
      if (!File.Exists(SYNC_FILE)) {
        last_sync_  = DateTime.UtcNow;
        basing_ = true;
      } else {
        string date_str = File.ReadAllText(SYNC_FILE);
        last_sync_ = DateTime.Parse(date_str);
      }
      Console.WriteLine($"Got last sync time: {last_sync_}.");
    }

    private void WalkVS(string root_dir) {
      DirectoryInfo root = new DirectoryInfo(root_dir);
      FileInfo[] files = root.GetFiles("*.*", SearchOption.AllDirectories);
      Console.WriteLine("Checking Visual Studio directory for file changes.");
      foreach(var file in files) {
        string p = Path.GetRelativePath(layout_dir_, file.Directory.FullName);
        string o = OUT_DIR + p + "/";
        if (file.LastWriteTimeUtc > last_sync_) {
          if (!Directory.Exists(o) && p != ".") {
            Directory.CreateDirectory(o);
          }
          File.Copy(file.FullName, o + file.Name);
        }
      }
    }
  }
}
