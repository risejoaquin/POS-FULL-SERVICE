using System;

namespace PosApplication.DTOs.Local
{
    public class SyncResult
    {
        public bool IsSuccess { get; set; }
        public string Message { get; set; } = string.Empty;
        public int RecordsUploaded { get; set; }
        public int RecordsDownloaded { get; set; }
        public DateTime SyncTime { get; set; }
    }
}
