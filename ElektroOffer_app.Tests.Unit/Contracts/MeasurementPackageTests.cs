using ElektroOffer.Contracts.Measurements;
using NUnit.Framework;
using System.Security.Cryptography;

namespace ElektroOffer_app.Tests.Unit.Contracts;

[TestFixture]
public sealed class MeasurementPackageTests
{
    [Test]
    public void TestData_IsValidAndSurvivesRoundTrip()
    {
        var path = Path.Combine(TestContext.CurrentContext.TestDirectory, "TestData", "Measurements", "valid-measurement.json");
        var package = MeasurementPackageSerializer.Deserialize(File.ReadAllText(path));

        var validation = MeasurementPackageValidator.Validate(package);
        var clone = MeasurementPackageSerializer.Deserialize(MeasurementPackageSerializer.Serialize(package));

        Assert.Multiple(() =>
        {
            Assert.That(validation.IsValid, Is.True, string.Join(Environment.NewLine, validation.Issues));
            Assert.That(clone.ExportId, Is.EqualTo(package.ExportId));
            Assert.That(clone.Project.Areas.Single().Items.Single().Quantity, Is.EqualTo(18.5m));
        });
    }

    [Test]
    public void Validator_RejectsUnsafeAttachmentAndInvalidQuantity()
    {
        var package = new MeasurementPackage
        {
            Project = new MeasurementProject
            {
                Name = "Test",
                Areas =
                [
                    new MeasurementArea
                    {
                        Name = "Mistnost",
                        Items = [new MeasurementItem { DisplayName = "Zasuvka", Quantity = 0, Unit = "ks" }]
                    }
                ]
            },
            Attachments =
            [
                new AttachmentReference { RelativePath = "../photo.jpg", Sha256 = "bad" }
            ]
        };

        var codes = MeasurementPackageValidator.Validate(package).Issues.Select(issue => issue.Code).ToArray();

        Assert.That(codes, Does.Contain("item.quantity.invalid"));
        Assert.That(codes, Does.Contain("attachment.path.unsafe"));
        Assert.That(codes, Does.Contain("attachment.hash.invalid"));
    }

    [Test]
    public async Task Archive_WritesManifestAndSeparateAttachment()
    {
        var bytes = "test-photo"u8.ToArray();
        var package = new MeasurementPackage
        {
            Project = new MeasurementProject { Name = "Archive test" },
            Attachments =
            [
                new AttachmentReference
                {
                    RelativePath = "attachments/photo.jpg",
                    ContentType = "image/jpeg",
                    Size = bytes.Length,
                    Sha256 = Convert.ToHexString(SHA256.HashData(bytes)).ToLowerInvariant()
                }
            ]
        };
        var path = Path.Combine(Path.GetTempPath(), $"measurement-{Guid.NewGuid():N}.eofmeasure");

        try
        {
            await MeasurementArchiveService.WriteAsync(package, path, (_, _) => Task.FromResult<Stream>(new MemoryStream(bytes)));
            var restored = await MeasurementArchiveService.ReadManifestAsync(path);
            var extractionRoot = Path.Combine(Path.GetTempPath(), $"measurement-extract-{Guid.NewGuid():N}");
            var extracted = await MeasurementArchiveService.ExtractAttachmentsAsync(path, extractionRoot);

            Assert.Multiple(() =>
            {
                Assert.That(restored.ExportId, Is.EqualTo(package.ExportId));
                Assert.That(restored.Attachments.Single().RelativePath, Is.EqualTo("attachments/photo.jpg"));
                Assert.That(new FileInfo(path).Length, Is.GreaterThan(bytes.Length));
                Assert.That(extracted, Has.Count.EqualTo(1));
                Assert.That(File.ReadAllBytes(extracted.Single()), Is.EqualTo(bytes));
            });
            Directory.Delete(extractionRoot, recursive: true);
        }
        finally
        {
            if (File.Exists(path))
                File.Delete(path);
        }
    }

    [Test]
    public void Archive_RejectsAttachmentWithChangedContent()
    {
        var declared = "original"u8.ToArray();
        var changed = "changed"u8.ToArray();
        var package = new MeasurementPackage
        {
            Project = new MeasurementProject { Name = "Checksum test" },
            Attachments =
            [
                new AttachmentReference
                {
                    RelativePath = "attachments/photo.jpg",
                    ContentType = "image/jpeg",
                    Size = declared.Length,
                    Sha256 = Convert.ToHexString(SHA256.HashData(declared)).ToLowerInvariant()
                }
            ]
        };
        var path = Path.Combine(Path.GetTempPath(), $"measurement-{Guid.NewGuid():N}.eofmeasure");

        Assert.That(
            async () => await MeasurementArchiveService.WriteAsync(package, path, (_, _) => Task.FromResult<Stream>(new MemoryStream(changed))),
            Throws.TypeOf<InvalidDataException>());
        Assert.That(File.Exists(path), Is.False);
    }
}
