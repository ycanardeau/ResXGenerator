using Aigamo.ResXGenerator.Tools;
using FluentAssertions;
using Xunit;

namespace Aigamo.ResXGenerator.Tests;

public class GroupResxFilesTests
{
	[Fact]
	public void CompareGroupedAdditionalFile_SameRoot_SameSubFiles_DifferentOrder()
	{
		var v1 = new GroupedAdditionalFile(new AdditionalTextWithHash(new AdditionalTextStub(
			@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.resx"), Guid.Parse("47FFD75C-3254-4851-8E1C-CBDDCDCE1D9B")),
			[
				new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.da.resx"), Guid.Parse("B7EDA261-6923-4526-AFB7-B2A64984F099")),
				new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.vi.resx"), Guid.Parse("5B2BA95C-FB9C-47C5-9C03-280B63D8DD27"))
			]);

		var v2 = new GroupedAdditionalFile(new AdditionalTextWithHash(new AdditionalTextStub(
				@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.resx"), Guid.Parse("47FFD75C-3254-4851-8E1C-CBDDCDCE1D9B")),
			[

				new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.vi.resx"), Guid.Parse("5B2BA95C-FB9C-47C5-9C03-280B63D8DD27")),
				new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.da.resx"), Guid.Parse("B7EDA261-6923-4526-AFB7-B2A64984F099"))
			]
		);
		v1.Should().Be(v2);
	}

	[Fact]
	public void CompareGroupedAdditionalFile_SameRoot_DiffSubFilesNames()
	{
		var v1 = new GroupedAdditionalFile(new AdditionalTextWithHash(new AdditionalTextStub(
				@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.resx"), Guid.Parse("47FFD75C-3254-4851-8E1C-CBDDCDCE1D9B")),
			[
				new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.en.resx"), Guid.Parse("B7EDA261-6923-4526-AFB7-B2A64984F099")),
				new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.fr.resx"), Guid.Parse("5B2BA95C-FB9C-47C5-9C03-280B63D8DD27"))
			]);

		var v2 = new GroupedAdditionalFile(new AdditionalTextWithHash(new AdditionalTextStub(
				@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.resx"), Guid.Parse("47FFD75C-3254-4851-8E1C-CBDDCDCE1D9B")),
			[
				new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.de.resx"), Guid.Parse("5B2BA95C-FB9C-47C5-9C03-280B63D8DD27")),
				new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.ro.resx"), Guid.Parse("B7EDA261-6923-4526-AFB7-B2A64984F099"))
			]
		);
		v1.Should().NotBe(v2);
	}

	[Fact]
	public void CompareGroupedAdditionalFile_SameRoot_DiffSubFileContent()
	{
		var v1 = new GroupedAdditionalFile(new AdditionalTextWithHash(new AdditionalTextStub(
				@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.resx"), Guid.Parse("47FFD75C-3254-4851-8E1C-CBDDCDCE1D9B")),
			[
				new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.da.resx"), Guid.Parse("771F9C76-D9F4-4AF4-95D2-B3426F9EC15A")),
				new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.vi.resx"), Guid.Parse("5B2BA95C-FB9C-47C5-9C03-280B63D8DD27"))
			]);

		var v2 = new GroupedAdditionalFile(new AdditionalTextWithHash(new AdditionalTextStub(
				@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.resx"), Guid.Parse("47FFD75C-3254-4851-8E1C-CBDDCDCE1D9B")),
			[
				new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.vi.resx"), Guid.Parse("5B2BA95C-FB9C-47C5-9C03-280B63D8DD27")),
				new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.da.resx"), Guid.Parse("B7EDA261-6923-4526-AFB7-B2A64984F099"))
			]
		);
		v1.Should().NotBe(v2);
	}

	[Fact]
	public void CompareGroupedAdditionalFile_DiffRootContent_SameSubFiles()
	{
		var v1 = new GroupedAdditionalFile(new AdditionalTextWithHash(new AdditionalTextStub(
				@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.resx"), Guid.Parse("47FFD75C-3254-4851-8E1C-CBDDCDCE1D9B")),
			[
				new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.da.resx"), Guid.Parse("B7EDA261-6923-4526-AFB7-B2A64984F099")),
				new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.vi.resx"), Guid.Parse("5B2BA95C-FB9C-47C5-9C03-280B63D8DD27"))
			]);

		var v2 = new GroupedAdditionalFile(new AdditionalTextWithHash(new AdditionalTextStub(
				@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.resx"), Guid.Parse("A7E92264-8047-4668-979F-6EFC14EBAFC5")),
			[

				new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.vi.resx"), Guid.Parse("5B2BA95C-FB9C-47C5-9C03-280B63D8DD27")),
				new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.da.resx"), Guid.Parse("B7EDA261-6923-4526-AFB7-B2A64984F099"))
			]
		);
		v1.Should().NotBe(v2);
	}

	static readonly (string Path, Guid Hash)[] s_data =
	[
		(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.da.resx", Guid.Parse("00000000-0000-0000-0000-000000000001")),
		(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.resx", Guid.Parse("00000000-0000-0000-0000-000000000002")),
		(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.vi.resx", Guid.Parse("00000000-0000-0000-0000-000000000003")),
		(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgLive.da.resx", Guid.Parse("00000000-0000-0000-0000-000000000004")),
		(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgLive.resx", Guid.Parse("00000000-0000-0000-0000-000000000005")),
		(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgLive.vi.resx", Guid.Parse("00000000-0000-0000-0000-000000000006")),
		(@"D:\src\xhg\y\Areas\Identity\Pages\Login.da.resx", Guid.Parse("00000000-0000-0000-0000-000000000007")),
		(@"D:\src\xhg\y\Areas\Identity\Pages\Login.resx", Guid.Parse("00000000-0000-0000-0000-000000000008")),
		(@"D:\src\xhg\y\Areas\Identity\Pages\Login.vi.resx", Guid.Parse("00000000-0000-0000-0000-000000000009")),
		(@"D:\src\xhg\y\Areas\QxModule\Pages\QasdLogon.da.resx", Guid.Parse("00000000-0000-0000-0000-000000000010")),
		(@"D:\src\xhg\y\Areas\QxModule\Pages\QasdLogon.resx", Guid.Parse("00000000-0000-0000-0000-000000000011")),
		(@"D:\src\xhg\y\Areas\QxModule\Pages\QasdLogon.vi.resx", Guid.Parse("00000000-0000-0000-0000-000000000012")),
		(@"D:\src\xhg\y\Areas\QxModule\QtrController.cs-cz.resx", Guid.Parse("00000000-0000-0000-0000-000000000013")),
		(@"D:\src\xhg\y\Areas\QxModule\QtrController.da.resx", Guid.Parse("00000000-0000-0000-0000-000000000014")),
		(@"D:\src\xhg\y\Areas\QxModule\QtrController.de.resx", Guid.Parse("00000000-0000-0000-0000-000000000015")),
		(@"D:\src\xhg\y\Areas\QxModule\QtrController.es.resx", Guid.Parse("00000000-0000-0000-0000-000000000016")),
		(@"D:\src\xhg\y\Areas\QxModule\QtrController.fi.resx", Guid.Parse("00000000-0000-0000-0000-000000000017")),
		(@"D:\src\xhg\y\Areas\QxModule\QtrController.fr.resx", Guid.Parse("00000000-0000-0000-0000-000000000018")),
		(@"D:\src\xhg\y\Areas\QxModule\QtrController.it.resx", Guid.Parse("00000000-0000-0000-0000-000000000019")),
		(@"D:\src\xhg\y\Areas\QxModule\QtrController.lt.resx", Guid.Parse("00000000-0000-0000-0000-000000000020")),
		(@"D:\src\xhg\y\Areas\QxModule\QtrController.lv.resx", Guid.Parse("00000000-0000-0000-0000-000000000021")),
		(@"D:\src\xhg\y\Areas\QxModule\QtrController.nb-no.resx", Guid.Parse("00000000-0000-0000-0000-000000000022")),
		(@"D:\src\xhg\y\Areas\QxModule\QtrController.nl.resx", Guid.Parse("00000000-0000-0000-0000-000000000023")),
		(@"D:\src\xhg\y\Areas\QxModule\QtrController.nn-no.resx", Guid.Parse("00000000-0000-0000-0000-000000000024")),
		(@"D:\src\xhg\y\Areas\QxModule\QtrController.pl.resx", Guid.Parse("00000000-0000-0000-0000-000000000025")),
		(@"D:\src\xhg\y\Areas\QxModule\QtrController.resx", Guid.Parse("00000000-0000-0000-0000-000000000026")),
		(@"D:\src\xhg\y\Areas\QxModule\QtrController.ru.resx", Guid.Parse("00000000-0000-0000-0000-000000000027")),
		(@"D:\src\xhg\y\Areas\QxModule\QtrController.sv.resx", Guid.Parse("00000000-0000-0000-0000-000000000028")),
		(@"D:\src\xhg\y\Areas\QxModule\QtrController.tr.resx", Guid.Parse("00000000-0000-0000-0000-000000000029")),
		(@"D:\src\xhg\y\Areas\QxModule\QtrController.vi.resx", Guid.Parse("00000000-0000-0000-0000-000000000030")),
		(@"D:\src\xhg\y\Areas\QxModule\QtrController.zh-cn.resx", Guid.Parse("00000000-0000-0000-0000-000000000031")),
		(@"D:\src\xhg\y\DataAnnotations\DataAnnotation.da.resx", Guid.Parse("00000000-0000-0000-0000-000000000032")),
		(@"D:\src\xhg\y\DataAnnotations\DataAnnotation.resx", Guid.Parse("00000000-0000-0000-0000-000000000033")),
		(@"D:\src\xhg\y\DataAnnotations\DataAnnotation2.resx", Guid.Parse("00000000-0000-0000-0000-000000000034"))
	];

	[Fact]
	public void FileGrouping()
	{
		var result = GroupResxFiles.Group(s_data.Select(x => new AdditionalTextWithHash(new AdditionalTextStub(x.Path), x.Hash)).OrderBy(_ => Guid.NewGuid()).ToArray());

		var testData = new List<GroupedAdditionalFile>
		{
			new(
				mainFile: new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.resx"), Guid.Parse("00000000-0000-0000-0000-000000000002")),
				subFiles:
				[
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.da.resx"), Guid.Parse("00000000-0000-0000-0000-000000000001")),
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgControlCenter.vi.resx"), Guid.Parse("00000000-0000-0000-0000-000000000003"))
				]
			),
			new(
				mainFile: new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgLive.resx"), Guid.Parse("00000000-0000-0000-0000-000000000005")),
				subFiles:
				[
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgLive.da.resx"), Guid.Parse("00000000-0000-0000-0000-000000000004")),
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\CaModule\Pages\IdfgLive.vi.resx"), Guid.Parse("00000000-0000-0000-0000-000000000006"))
				]
			),
			new(
				mainFile: new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\Identity\Pages\Login.resx"), Guid.Parse("00000000-0000-0000-0000-000000000008")),
				subFiles:
				[
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\Identity\Pages\Login.da.resx"), Guid.Parse("00000000-0000-0000-0000-000000000007")),
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\Identity\Pages\Login.vi.resx"), Guid.Parse("00000000-0000-0000-0000-000000000009"))
				]
			),
			new(
				mainFile: new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\QxModule\Pages\QasdLogon.resx"), Guid.Parse("00000000-0000-0000-0000-000000000011")),
				subFiles:
				[
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\QxModule\Pages\QasdLogon.da.resx"), Guid.Parse("00000000-0000-0000-0000-000000000010")),
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\QxModule\Pages\QasdLogon.vi.resx"), Guid.Parse("00000000-0000-0000-0000-000000000012"))
				]
			),
			new(
				mainFile: new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\QxModule\QtrController.resx"), Guid.Parse("00000000-0000-0000-0000-000000000026")),
				subFiles:
				[
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\QxModule\QtrController.cs-cz.resx"), Guid.Parse("00000000-0000-0000-0000-000000000013")),
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\QxModule\QtrController.da.resx"), Guid.Parse("00000000-0000-0000-0000-000000000014")),
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\QxModule\QtrController.de.resx"), Guid.Parse("00000000-0000-0000-0000-000000000015")),
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\QxModule\QtrController.es.resx"), Guid.Parse("00000000-0000-0000-0000-000000000016")),
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\QxModule\QtrController.fi.resx"), Guid.Parse("00000000-0000-0000-0000-000000000017")),
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\QxModule\QtrController.fr.resx"), Guid.Parse("00000000-0000-0000-0000-000000000018")),
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\QxModule\QtrController.it.resx"), Guid.Parse("00000000-0000-0000-0000-000000000019")),
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\QxModule\QtrController.lt.resx"), Guid.Parse("00000000-0000-0000-0000-000000000020")),
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\QxModule\QtrController.lv.resx"), Guid.Parse("00000000-0000-0000-0000-000000000021")),
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\QxModule\QtrController.nb-no.resx"), Guid.Parse("00000000-0000-0000-0000-000000000022")),
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\QxModule\QtrController.nl.resx"), Guid.Parse("00000000-0000-0000-0000-000000000023")),
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\QxModule\QtrController.nn-no.resx"), Guid.Parse("00000000-0000-0000-0000-000000000024")),
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\QxModule\QtrController.pl.resx"), Guid.Parse("00000000-0000-0000-0000-000000000025")),
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\QxModule\QtrController.ru.resx"), Guid.Parse("00000000-0000-0000-0000-000000000027")),
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\QxModule\QtrController.sv.resx"), Guid.Parse("00000000-0000-0000-0000-000000000028")),
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\QxModule\QtrController.tr.resx"), Guid.Parse("00000000-0000-0000-0000-000000000029")),
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\QxModule\QtrController.vi.resx"), Guid.Parse("00000000-0000-0000-0000-000000000030")),
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\Areas\QxModule\QtrController.zh-cn.resx"), Guid.Parse("00000000-0000-0000-0000-000000000031"))
				]),
			new(
				mainFile: new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\DataAnnotations\DataAnnotation.resx"), Guid.Parse("00000000-0000-0000-0000-000000000033")),
				subFiles:
				[
					new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\DataAnnotations\DataAnnotation.da.resx"), Guid.Parse("00000000-0000-0000-0000-000000000032"))
				]
			),
			new(
				mainFile: new AdditionalTextWithHash(new AdditionalTextStub(@"D:\src\xhg\y\DataAnnotations\DataAnnotation2.resx"), Guid.Parse("00000000-0000-0000-0000-000000000034")),
				subFiles: []
			)
		};
		var resAsList = result.ToList();
		resAsList.Count.Should().Be(testData.Count);
		testData.ForEach(groupedAdditionalFile => resAsList.Should().Contain(groupedAdditionalFile));
	}

	[Fact]
	public void ResxGrouping()
	{
		var result = GroupResxFiles.DetectChildCombos(GroupResxFiles.Group(s_data.Select(x => new AdditionalTextWithHash(new AdditionalTextStub(x.Path), Guid.NewGuid())).OrderBy(_ => Guid.NewGuid()).ToArray()).ToArray()).ToList();
		var expected = new List<CultureInfoCombo>
		{
			new([
				new AdditionalTextWithHash(new AdditionalTextStub("test.da.resx"), Guid.NewGuid()),
				new AdditionalTextWithHash(new AdditionalTextStub("test.vi.resx"), Guid.NewGuid())
			]),
			new([new AdditionalTextWithHash(new AdditionalTextStub("test.da.resx"), Guid.NewGuid())]),
			new([]),
			new([
				new AdditionalTextWithHash(new AdditionalTextStub("test.cs-cz.resx"), Guid.NewGuid()),
				new AdditionalTextWithHash(new AdditionalTextStub("test.da.resx"), Guid.NewGuid()),
				new AdditionalTextWithHash(new AdditionalTextStub("test.de.resx"), Guid.NewGuid()),
				new AdditionalTextWithHash(new AdditionalTextStub("test.es.resx"), Guid.NewGuid()),
				new AdditionalTextWithHash(new AdditionalTextStub("test.fi.resx"), Guid.NewGuid()),
				new AdditionalTextWithHash(new AdditionalTextStub("test.fr.resx"), Guid.NewGuid()),
				new AdditionalTextWithHash(new AdditionalTextStub("test.it.resx"), Guid.NewGuid()),
				new AdditionalTextWithHash(new AdditionalTextStub("test.lt.resx"), Guid.NewGuid()),
				new AdditionalTextWithHash(new AdditionalTextStub("test.lv.resx"), Guid.NewGuid()),
				new AdditionalTextWithHash(new AdditionalTextStub("test.nb-no.resx"), Guid.NewGuid()),
				new AdditionalTextWithHash(new AdditionalTextStub("test.nl.resx"), Guid.NewGuid()),
				new AdditionalTextWithHash(new AdditionalTextStub("test.nn-no.resx"), Guid.NewGuid()),
				new AdditionalTextWithHash(new AdditionalTextStub("test.pl.resx"), Guid.NewGuid()),
				new AdditionalTextWithHash(new AdditionalTextStub("test.ru.resx"), Guid.NewGuid()),
				new AdditionalTextWithHash(new AdditionalTextStub("test.sv.resx"), Guid.NewGuid()),
				new AdditionalTextWithHash(new AdditionalTextStub("test.tr.resx"), Guid.NewGuid()),
				new AdditionalTextWithHash(new AdditionalTextStub("test.vi.resx"), Guid.NewGuid()),
				new AdditionalTextWithHash(new AdditionalTextStub("test.zh-cn.resx"), Guid.NewGuid())
			]),
		};
		result.Count.Should().Be(expected.Count);
		result.Should().BeEquivalentTo(expected);
	}
}


