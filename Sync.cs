using System;
using System.IO;
using System.IO.Compression;
using System.Diagnostics;

namespace Sync {
  public class Syncer {
    private DateTime last_sync_;
    /* If this is a first run (creating a base) */
    private bool basing_ = false;
    
    private readonly string SYNC_FILE = "./sync_time";
    private readonly string DELTA_DIR = "./DELTA/";
    private readonly string ARCHIVE_DIR = "./ARCHIVE/";
    private string args_ = File.ReadAllText("ARGS.txt");
    private string layout_dir_ = null;

    public Syncer() {
      GetSyncTime();
      GetLayoutdir();
      RunVSDownload();
      if (!basing_) {
        MkDeltaDir();
        WalkVSTree();
      }
      SetSyncTime();
      ZipDelta();
    } 

    private void GetLayoutdir() {
      /* TODO: If layout path contains spaces, we are fucked, should fix. */
      string[] a = args_.Split(" ");
      for(int i = 0; i < a.Length; i++) {
        if (a[i] == "--layout") {
          layout_dir_ = a[i+1];
          break;
        }
      }

      if (this.layout_dir_ == null) {
        throw new ArgumentNullException("Layout argument not found.");
      }
    }

    private void ZipDelta() {
      Console.WriteLine("Zipping Delta Files.");
      if (!Directory.Exists(ARCHIVE_DIR)) {
        Directory.CreateDirectory(ARCHIVE_DIR);
      }
      ZipFile.CreateFromDirectory(
        DELTA_DIR,
        ARCHIVE_DIR + $"{last_sync_.ToString("dd_MM_yyyy_HHmmss")}.zip"
      );
      RmDeltaDir();
    }


    private void MkDeltaDir() {
      RmDeltaDir();
      Directory.CreateDirectory(DELTA_DIR);
    }
    
    private void RmDeltaDir() {
      if (Directory.Exists(DELTA_DIR)) {
        Directory.Delete(DELTA_DIR, true);
      }
    }

    private void RunVSDownload() {
      Console.WriteLine("Starting Visual Studio Download / Syncing");
      var p = Process.Start("./vs.exe", args_);
      p.WaitForExit(); 
    }

    private void SetSyncTime() {
      last_sync_ = DateTime.UtcNow;
      File.WriteAllText(SYNC_FILE, last_sync_.ToString());
      Console.WriteLine($"Setting last sync time: {last_sync_}.");
    }

    private void GetSyncTime() {
      if (!File.Exists(SYNC_FILE)) {
        last_sync_  = DateTime.UtcNow;
        basing_ = true;
      } else {
        string date_str = File.ReadAllText(SYNC_FILE);
        last_sync_ = DateTime.Parse(date_str);
      }
      Console.WriteLine($"Got last sync time: {last_sync_}.");
    }

    private void WalkVSTree() {
      Console.WriteLine("Checking Visual Studio directory for file changes.");
      DirectoryInfo root = new DirectoryInfo(layout_dir_);
      FileInfo[] files = root.GetFiles("*.*", SearchOption.AllDirectories);
      foreach(var file in files) {
        if (file.LastWriteTimeUtc > last_sync_) {
          this.CopyFileToOut(file);
        }
      }
    }

    private void CopyFileToOut(FileInfo file) {
      string p = Path.GetRelativePath(layout_dir_, file.Directory.FullName);
      string o = DELTA_DIR + p + "/";
      if (!Directory.Exists(o) && p != ".") {
        Directory.CreateDirectory(o);
      }
      File.Copy(file.FullName, o + file.Name);
    }
  }
}
