using JetBrains.Annotations;
using Radish.ContentBuilder.AssetProcessors;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace Radish.ContentBuilder.StandardAssetProcessors;

/// <summary>
/// Standard asset processor for processing image formats into runtime-suitable textures.
/// Supports various pre-processing options such as premultiplying the alpha or resizing the output image.
/// </summary>
[PublicAPI]
public sealed class TextureProcessor : AssetProcessor
{
    /// <summary>
    /// The format to export the texture as.
    /// </summary>
    [PublicAPI]
    public enum OutputTextureFormat
    {
        /// <summary>
        /// Use QOI images as the output.
        /// These are a bit larger than png but decompress faster.
        /// </summary>
        Qoi,
        /// <summary>
        /// Use PNG images as the output.
        /// These have better compression than QOI but are slightly slower to decompress.
        /// </summary>
        Png
    }

    /// <summary>
    /// The format to write textures in.
    /// </summary>
    public OutputTextureFormat Format { get; init; } = OutputTextureFormat.Qoi;
    
    /// <summary>
    /// If <see langword="true"/>, the texture's colour will be premultiplied by the image alpha channel.
    /// </summary>
    public bool PremultiplyAlpha { get; init; } = true;
    
    /// <summary>
    /// If not 1, the texture will be up/down scaled by the given factor.
    /// </summary>
    public decimal RescaleFactor { get; init; } = 1;
    
    /// <inheritdoc/>
    public override async Task<AssetProcessorResult> ProcessContentFile(AssetProcessorInput input)
    {
        using var img = await Image.LoadAsync<Rgba32>(input.ContentFile.FullName);

        if (RescaleFactor != 1)
        {
            img.Mutate(op =>
            {
                var sz = op.GetCurrentSize();
                var resampler = RescaleFactor < 1 ? KnownResamplers.Lanczos3 : KnownResamplers.Bicubic;
                var newSize = new Size((int)(sz.Width * RescaleFactor), (int)(sz.Height * RescaleFactor));
                op.Resize(new ResizeOptions
                {
                    Mode = ResizeMode.Stretch,
                    Size = newSize,
                    Sampler = resampler
                });
            });
        }

        var outFile = MakeOutputFileFromInput(input, Format switch
        {
            OutputTextureFormat.Qoi => ".qoi",
            OutputTextureFormat.Png => ".png",
            _ => throw new ArgumentException("Unknown output texture format")
        });
        
        EnsureFileDirectoryExists(outFile);

        switch (Format)
        {
            case OutputTextureFormat.Qoi:
                await img.SaveAsQoiAsync(outFile.FullName);
                break;
            case OutputTextureFormat.Png:
                await img.SaveAsPngAsync(outFile.FullName);
                break;
            default:
                throw new ArgumentException("Unknown output texture format");
        }

        return new AssetProcessorResult([outFile.FullName]);
    }
}