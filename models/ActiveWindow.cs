namespace Models {
    public class ActiveWindow {
        public IntPtr hwnd { get; set; }
        public uint pid { get;set; }
        public string? WindowName{ get; set; }
        public string? ProcessName { get; set; }
    }
}