using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;

namespace SabaBot.Utils;

internal static class PatGifMaker {
    static PatGifMaker() {
        for (var i = 0; i < PatImages.Length; i++) {
            var pet = Image.Load<Rgba32>($"img/pet{i}.gif");
            pet.Mutate(static x => x.Resize(Resolution));
            PatImages[i] = pet;
        }
    }

    private const int Frames = 10;
    private const int Delay = 20;

    private static readonly Size Resolution = new(128, 128);
    private static readonly Image<Rgba32>[] PatImages = new Image<Rgba32>[Frames];

    public static Image Make(Image image) {
        var images = new List<Image<Rgba32>>();
        using var baseImage = image.Clone(x => x.Resize(Resolution));
        for (var i = 0; i < Frames; i++) {
            CalculateSqueeze(
                i,
                Frames,
                out var width,
                out var height,
                out var offsetX,
                out var offsetY
            );

            var canvas = new Image<Rgba32>(Resolution.Width, Resolution.Height);
            var clone = baseImage.Clone(
                x => x.Resize(
                    (int)(width * Resolution.Width),
                    (int)(height * Resolution.Height)
                )
            );
            var point = new Point(
                (int)(offsetX * Resolution.Width),
                (int)(offsetY * Resolution.Height)
            );

            canvas.Mutate(x => x.DrawImage(clone, point, 1));
            canvas.Mutate(x => x.DrawImage(PatImages[i], 1));

            images.Add(canvas);
        }

        return MakeGif(images, Delay);
    }

    private static Image<Rgba32> MakeGif(IReadOnlyList<Image<Rgba32>> images, int delay) {
        var gif = new Image<Rgba32>(images[0].Width, images[0].Height);

        foreach (var img in images) {
            var frame = img.Frames.RootFrame;
            var meta = frame.Metadata.GetGifMetadata();
            meta.FrameDelay = delay / 10;
            meta.DisposalMethod = GifDisposalMethod.RestoreToBackground;
            gif.Frames.AddFrame(frame);
        }

        gif.Metadata.GetGifMetadata().RepeatCount = 0;
        gif.Frames.RemoveFrame(0);
        return gif;
    }

    private static void CalculateSqueeze(
        int index,
        int framesCount,
        out float width,
        out float height,
        out float offsetX,
        out float offsetY
    ) {
        var squeeze = index < framesCount / 2 ? index : framesCount - index;
        width = 0.8f + squeeze * 0.02f;
        height = 0.8f - squeeze * 0.05f;
        offsetX = (1 - width) * 0.5f + 0.1f;
        offsetY = (1 - height) - 0.08f;
    }
}