
namespace UNEB
{
    public static class StringExtensions
    {

        /// <summary>
        /// Merges the parent and child paths with the '/' character.
        /// </summary>
        /// <param name="parentDir"></param>
        /// <param name="childDir"></param>
        /// <returns></returns>
        public static string Dir(this string parentDir, string childDir)
        {
            return parentDir + '/' + childDir;
        }

        /// <summary>
        /// Appends the extension to the file name with '.'
        /// </summary>
        /// <param name="file"></param>
        /// <param name="extension"></param>
        /// <returns></returns>
        public static string Ext(this string file, string extension)
        {
            return file + '.' + extension;
        }
    }
}