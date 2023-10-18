using System;
using Windows.ApplicationModel;

namespace WinGetStore.Models
{
    public record struct SystemVersionInfo(int Major, int Minor, int Build, int Revision = 0) : IComparable, IComparable<SystemVersionInfo>
    {
        public readonly bool Equals(SystemVersionInfo other) => Major == other.Major && Minor == other.Minor && Build == other.Build && Revision == other.Revision;

        public override readonly int GetHashCode() => Major.GetHashCode() ^ Minor.GetHashCode() ^ Build.GetHashCode() ^ Revision.GetHashCode();

        public readonly int CompareTo(SystemVersionInfo other) =>
            Major != other.Major
                ? Major.CompareTo(other.Major)
                : Minor != other.Minor
                ? Minor.CompareTo(other.Minor)
                : Build != other.Build ? Build.CompareTo(other.Build) : Revision != other.Revision ? Revision.CompareTo(other.Revision) : 0;

        public readonly int CompareTo(object obj) => obj is SystemVersionInfo other ? CompareTo(other) : throw new ArgumentException();

        public static bool operator <(SystemVersionInfo left, SystemVersionInfo right) => left.CompareTo(right) < 0;

        public static bool operator <=(SystemVersionInfo left, SystemVersionInfo right) => left.CompareTo(right) <= 0;

        public static bool operator >(SystemVersionInfo left, SystemVersionInfo right) => left.CompareTo(right) > 0;

        public static bool operator >=(SystemVersionInfo left, SystemVersionInfo right) => left.CompareTo(right) >= 0;

        /// <summary>
        /// Returns a string representation of a version with the format 'Major.Minor.Build.Revision'.
        /// </summary>
        /// <param name="significance">The number of version numbers to return, default is 4 for the full version number.</param>
        /// <returns>Version string of the format 'Major.Minor.Build.Revision'</returns>
        public readonly string ToString(int significance = 4) => significance switch
        {
            4 => $"{Major}.{Minor}.{Build}.{Revision}",
            3 => $"{Major}.{Minor}.{Build}",
            2 => $"{Major}.{Minor}",
            1 => $"{Major}",
            _ => throw new ArgumentOutOfRangeException(nameof(significance), "Value must be a value 1 through 4."),
        };

        public override readonly string ToString() => $"{Major}.{Minor}.{Build}.{Revision}";

        public static implicit operator SystemVersionInfo(Version version) => new(version.Major, version.Minor, version.Build, version.Revision);

        public static implicit operator SystemVersionInfo(PackageVersion version) => new(version.Major, version.Minor, version.Build, version.Revision);

        public static implicit operator Version(SystemVersionInfo version) => new(version.Major, version.Minor, version.Build, version.Revision);

        public static explicit operator PackageVersion(SystemVersionInfo version) => new() { Major = (ushort)version.Major, Minor = (ushort)version.Minor, Build = (ushort)version.Build, Revision = (ushort)version.Revision };
    }
}
