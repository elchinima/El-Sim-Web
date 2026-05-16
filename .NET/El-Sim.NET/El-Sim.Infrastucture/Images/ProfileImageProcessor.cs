namespace El_Sim.Infrastucture.Images;

public class ProfileImageProcessor
{
    private const int Size = 512;

    public async Task SaveWebpAsync(Stream input, string outputPath, CancellationToken cancellationToken = default)
    {
        using var image = await Image.LoadAsync(input, cancellationToken);
        var side = Math.Min(image.Width, image.Height);
        var x = (image.Width - side) / 2;
        var y = (image.Height - side) / 2;

        image.Mutate(context => context.Crop(new Rectangle(x, y, side, side)).Resize(Size, Size));

        await image.SaveAsWebpAsync(outputPath, new WebpEncoder { Quality = 50 }, cancellationToken);
    }
}
