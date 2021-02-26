namespace PdfConverter.Simple.StreamDecoding
{
    /// <summary>
    /// Decodes compressed stream contents
    /// </summary>
    public interface IStreamDecoder
    {
        /// <summary>
        /// Decode compressed data
        /// </summary>
        /// <param name="inputData">Compressed data bytes</param>
        /// <returns>Decompressed data bytes</returns>
        byte[] Decode(byte[] inputData);
    }
}