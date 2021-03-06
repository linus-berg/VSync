using System.IO;
using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.IO.Compression;
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
      var p = Process.Start("./vs.exe", args_);
      p.WaitForExit(); 
    }

    private void SetLastSyncTime() {
      last_sync_ = DateTime.UtcNow;
      File.WriteAllText(SYNC_FILE, last_sync_.ToString());
    }

    private void GetLastSyncTime() {
      if (!File.Exists(SYNC_FILE)) {
        last_sync_  = DateTime.UtcNow;
        basing_ = true;
      } else {
        string date_str = File.ReadAllText(SYNC_FILE);
        last_sync_ = DateTime.Parse(date_str);
      }
    }

    private void WalkVS(string root_dir) {
      DirectoryInfo root = new DirectoryInfo(root_dir);
      FileInfo[] files = root.GetFiles("*.*", SearchOption.AllDirectories);
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