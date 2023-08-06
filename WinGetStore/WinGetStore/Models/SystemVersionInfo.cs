using System;

namespace WinGetStore.Models
{
    public readonly struct SystemVersionInfo(int major, int minor, int build, int revision = 0)
    {
        public int Major { get; } = major;

        public int Minor { get; } = minor;

        public int Build { get; } = build;

        public int Revision { get; } = revision;

        public bool Equals(SystemVersionInfo other) => Major == other.Major && Minor == other.Minor && Build == other.Build && Revision == other.Revision;

        public override bool Equals(object obj) => obj is SystemVersionInfo other && Equals(other);

        public override int GetHashCode() => Major.GetHashCode() ^ Minor.GetHashCode() ^ Build.GetHashCode() ^ Revision.GetHashCode();

        public static bool operator ==(SystemVersionInfo left, SystemVersionInfo right) => left.Equals(right);

        public static bool operator !=(SystemVersionInfo left, SystemVersionInfo right) => !(left == right);

        public int CompareTo(SystemVersionInfo other) =>
            Major != other.Major
                ? Major.CompareTo(other.Major)
                : Minor != other.Minor
                    ? Minor.CompareTo(other.Minor)
                    : Build != other.Build
                        ? Build.CompareTo(other.Build)
                        : Revision != other.Revision
                            ? Revision.CompareTo(other.Revision)
                            : 0;

        public int CompareTo(object obj) => obj is SystemVersionInfo other ? CompareTo(other) : throw new ArgumentException();

        public static bool operator <(SystemVersionInfo left, SystemVersionInfo right) => left.CompareTo(right) < 0;

        public static bool operator <=(SystemVersionInfo left, SystemVersionInfo right) => left.CompareTo(right) <= 0;

        public static bool operator >(SystemVersionInfo left, SystemVersionInfo right) => left.CompareTo(right) > 0;

        public static bool operator >=(SystemVersionInfo left, SystemVersionInfo right) => left.CompareTo(right) >= 0;

        public override string ToString() => $"{Major}.{Minor}.{Build}.{Revision}";
    }
}
