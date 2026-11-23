using System.Text.Json;
using System.Text.Json.Nodes;
using Screen.Core.Contracts;
using Xunit;

namespace Screen.Core.Tests.Contracts;

public sealed class VisionInspectResultTests
{
    private static readonly string _examplePath =
        Path.Combine(AppContext.BaseDirectory, "contracts", "examples", "vision-inspect-result.json");

    [Fact]
    public void TheExampleIsRead()
    {
        VisionInspectResult result = Read();

        Assert.Equal("insp-2026-11-20T21-14-07Z-A-0142", result.InspectionId);
        Assert.Equal(new DateTimeOffset(2026, 11, 20, 21, 14, 7, 412, TimeSpan.Zero), result.CapturedAt);
        Assert.Equal([3840, 2160], result.Camera.Resolution);
        Assert.Equal(["scratch", "missing_stud"], result.Detections.Select(d => d.Label));
        Assert.Equal([1412, 820, 1690, 905], result.Detections[0].BboxXyxy);
        Assert.Equal(2, result.Images.Crops.Count);
    }

    [Fact]
    public void TheExampleRoundTrips()
    {
        JsonNode original = JsonNode.Parse(File.ReadAllText(_examplePath))!;
        JsonNode written = JsonSerializer.SerializeToNode(Read(), ContractJson.Options)!;

        Assert.True(JsonNode.DeepEquals(original, written), $"expected {original.ToJsonString()}, got {written.ToJsonString()}");
    }

    [Theory]
    [InlineData("2026-11-20T21:14:07Z", "2026-11-20T21:14:07Z")]
    [InlineData("2026-11-20T21:14:07.4120000Z", "2026-11-20T21:14:07.412Z")]
    [InlineData("2026-11-20T21:14:07.0000001Z", "2026-11-20T21:14:07.0000001Z")]
    public void CaptureTimeIsWrittenInUtcWithZ(string read, string written)
    {
        JsonObject input = JsonNode.Parse(File.ReadAllText(_examplePath))!.AsObject();
        input["captured_at"] = read;

        VisionInspectResult result = JsonSerializer.Deserialize<VisionInspectResult>(input.ToJsonString(), ContractJson.Options)!;

        Assert.Equal(written, JsonSerializer.SerializeToNode(result, ContractJson.Options)!["captured_at"]!.GetValue<string>());
    }

    [Fact]
    public void ACaptureTimeWithAnOffsetIsWrittenAsTheSameInstantInUtc()
    {
        VisionInspectResult example = Read();
        var local = new DateTimeOffset(2026, 11, 20, 22, 14, 7, 412, TimeSpan.FromHours(1));
        var result = new VisionInspectResult(
            example.Camera, local, example.Detections, example.Detector, example.Images, example.InspectionId, example.PartId);

        JsonNode written = JsonSerializer.SerializeToNode(result, ContractJson.Options)!;

        Assert.Equal("2026-11-20T21:14:07.412Z", written["captured_at"]!.GetValue<string>());
    }

    [Theory]
    [InlineData("missing_inspection_id")]
    [InlineData("null_inspection_id")]
    [InlineData("extra_field")]
    [InlineData("null_crop")]
    [InlineData("null_detection")]
    [InlineData("captured_at_with_offset")]
    [InlineData("captured_at_not_a_time")]
    public void StrictReadingRefusesBadInput(string breakage)
    {
        JsonObject bad = JsonNode.Parse(File.ReadAllText(_examplePath))!.AsObject();
        switch (breakage)
        {
            case "missing_inspection_id":
                bad.Remove("inspection_id");
                break;
            case "null_inspection_id":
                bad["inspection_id"] = null;
                break;
            case "extra_field":
                bad["unexpected"] = 1;
                break;
            case "null_crop":
                bad["images"]!["crops"]!.AsArray()[0] = null;
                break;
            case "null_detection":
                bad["detections"]!.AsArray()[0] = null;
                break;
            case "captured_at_with_offset":
                bad["captured_at"] = "2026-11-20T22:14:07.412+01:00";
                break;
            case "captured_at_not_a_time":
                bad["captured_at"] = "2026-13-40T99:00:00Z";
                break;
            default:
                throw new ArgumentOutOfRangeException(nameof(breakage), breakage, "unknown breakage");
        }

        Assert.Throws<JsonException>(() => JsonSerializer.Deserialize<VisionInspectResult>(bad.ToJsonString(), ContractJson.Options));
    }

    private static VisionInspectResult Read() =>
        JsonSerializer.Deserialize<VisionInspectResult>(File.ReadAllText(_examplePath), ContractJson.Options)
        ?? throw new InvalidOperationException("the example is null");
}
