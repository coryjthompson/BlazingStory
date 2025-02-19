﻿using BlazingStory.Internals.Models;
using BlazingStory.Internals.Services.Navigation;
using BlazingStory.Test._Fixtures;
using BlazingStoryApp1.Stories;
using RazorClassLib1.Components.Button;
using RazorClassLib1.Components.Rating;
using RazorClassLib1.Components.Select;
using static BlazingStory.Test._Fixtures.TestHelper;

namespace BlazingStory.Test.Internals.Services.Navigation;

internal class NavigationTreeBuilderTest
{
    [Test]
    public void Build_from_EmptyBook_Test()
    {
        // Given
        var builder = new NavigationTreeBuilder();
        var storyContainers = Array.Empty<StoryContainer>();

        // When
        var root = builder.Build(storyContainers, null);

        // Then
        root.Type.Is(NavigationItemType.Container);
        root.SubItems.Count.Is(0);
    }

    [Test]
    public async Task Build_Test()
    {
        // Given
        await using var host = new TestHost();
        var builder = new NavigationTreeBuilder();
        var storyContainers = new StoryContainer[] {
            new(typeof(Button), null, new(typeof(Button_stories), new("Examples/Button")), host.Services) { Stories = {
                new(Descriptor("Examples/Button"), "Default", StoryContext.CreateEmpty(), null, null, EmptyFragment),
                new(Descriptor("Examples/Button"), "Primary", StoryContext.CreateEmpty(), null, null, EmptyFragment),
            }},
            new(typeof(Button), null, new(typeof(Select_stories), new("Examples/Select")), host.Services) { Stories = {
                new(Descriptor("Examples/Select"), "Select", StoryContext.CreateEmpty(), null, null, EmptyFragment),
            }}
        };

        // When
        var root = builder.Build(storyContainers, expandedNavigationPath: "/story/examples-button--primary");

        // Then
        root.Type.Is(NavigationItemType.Container);
        root.SubItems.Count.Is(1);

        var examplesNode = root.SubItems[0];
        examplesNode.Type.Is(NavigationItemType.Container);
        examplesNode.Caption.Is("Examples");
        examplesNode.SubItems.Count.Is(2);
        examplesNode.Expanded.IsTrue();
        examplesNode.PathSegments.Count().Is(0);

        var buttonNode = examplesNode.SubItems[0];
        buttonNode.Type.Is(NavigationItemType.Component);
        buttonNode.Caption.Is("Button");
        buttonNode.SubItems.Count.Is(3);
        buttonNode.Expanded.IsTrue();
        buttonNode.PathSegments.Is("Examples");

        var docsOfButtonNode = buttonNode.SubItems[0];
        docsOfButtonNode.Type.Is(NavigationItemType.Docs);
        docsOfButtonNode.Caption.Is("Docs");
        docsOfButtonNode.NavigationPath.Is("/docs/examples-button--docs");
        docsOfButtonNode.SubItems.Count.Is(0);
        docsOfButtonNode.PathSegments.Is("Examples", "Button");

        var defaultButtonNode = buttonNode.SubItems[1];
        defaultButtonNode.Type.Is(NavigationItemType.Story);
        defaultButtonNode.Caption.Is("Default");
        defaultButtonNode.NavigationPath.Is("/story/examples-button--default");
        defaultButtonNode.SubItems.Count.Is(0);
        defaultButtonNode.PathSegments.Is("Examples", "Button");

        var primaryButtonNode = buttonNode.SubItems[2];
        primaryButtonNode.Type.Is(NavigationItemType.Story);
        primaryButtonNode.Caption.Is("Primary");
        primaryButtonNode.NavigationPath.Is("/story/examples-button--primary");
        primaryButtonNode.SubItems.Count.Is(0);
        primaryButtonNode.PathSegments.Is("Examples", "Button");

        var selectNode = examplesNode.SubItems[1];
        selectNode.Type.Is(NavigationItemType.Component);
        selectNode.Caption.Is("Select");
        selectNode.SubItems.Count.Is(2);
        selectNode.Expanded.IsFalse();
        selectNode.PathSegments.Is("Examples");

        var docsOfSelectNode = selectNode.SubItems[0];
        docsOfSelectNode.Type.Is(NavigationItemType.Docs);
        docsOfSelectNode.Caption.Is("Docs");
        docsOfSelectNode.NavigationPath.Is("/docs/examples-select--docs");
        docsOfSelectNode.SubItems.Count.Is(0);
        docsOfSelectNode.PathSegments.Is("Examples", "Select");

        var selectSelectNode = selectNode.SubItems[1];
        selectSelectNode.Type.Is(NavigationItemType.Story);
        selectSelectNode.Caption.Is("Select");
        selectSelectNode.NavigationPath.Is("/story/examples-select--select");
        selectSelectNode.SubItems.Count.Is(0);
        selectSelectNode.PathSegments.Is("Examples", "Select");
    }

    [Test]
    public async Task Build_SubHeadings_Should_Be_Expanded_Test()
    {
        // Given
        await using var host = new TestHost();
        var builder = new NavigationTreeBuilder();
        var storyContainers = new StoryContainer[] {
            new(typeof(Button), null, new(typeof(Button_stories), new("Components/Button")), host.Services){ Stories = {
                new(Descriptor("Components/Button"), "Default", StoryContext.CreateEmpty(), null, null, EmptyFragment),
                new(Descriptor("Components/Button"), "Primary", StoryContext.CreateEmpty(), null, null, EmptyFragment),
            }},
            new(typeof(Button), null, new(typeof(Select_stories), new("Pages/Authentication")), host.Services){ Stories = {
                new(Descriptor("Pages/Authentication"), "Sign In", StoryContext.CreateEmpty(), null, null, EmptyFragment),
                new(Descriptor("Pages/Authentication"), "Sign Out", StoryContext.CreateEmpty(), null, null, EmptyFragment),
            }},
        };

        // When
        var root = builder.Build(storyContainers, null);

        // Then
        var componentsNode = root.SubItems[0];
        componentsNode.Expanded.IsTrue();
        var buttonNode = componentsNode.SubItems[0];
        buttonNode.Expanded.IsFalse();

        var pagesNode = root.SubItems[1];
        pagesNode.Expanded.IsTrue();
        var authNode = pagesNode.SubItems[0];
        authNode.Expanded.IsFalse();
    }

    [Test]
    public async Task Build_and_its_Ordering_Test()
    {
        // Given
        await using var host = new TestHost();
        var storyContainers = new StoryContainer[] {
            new(typeof(Rating), null, new(typeof(Select_stories), new("UI Components/Atoms/Rating")), host.Services) { Stories = {
                new(Descriptor("UI Components/Atoms/Rating"), "Default", StoryContext.CreateEmpty(), null, null, EmptyFragment),
            }},
            new(typeof(Select), null, new(typeof(Select_stories), new("Examples/Select")), host.Services) { Stories = {
                new(Descriptor("Examples/Select"), "Single Select", StoryContext.CreateEmpty(), null, null, EmptyFragment),
                new(Descriptor("Examples/Select"), "Multiple Select", StoryContext.CreateEmpty(), null, null, EmptyFragment),
            }},
            new(typeof(Button), null, new(typeof(Button_stories), new("Examples/Button")), host.Services) { Stories = {
                new(Descriptor("Examples/Button"), "Default", StoryContext.CreateEmpty(), null, null, EmptyFragment),
                new(Descriptor("Examples/Button"), "Primary", StoryContext.CreateEmpty(), null, null, EmptyFragment),
                new(Descriptor("Examples/Button"), "Danger", StoryContext.CreateEmpty(), null, null, EmptyFragment),
            }},
        };

        // When
        var builder = new NavigationTreeBuilder();
        var root = builder.Build(storyContainers, expandedNavigationPath: null);

        // Then
        root.SubItems.Select(node => node.Caption).Is("Examples", "UI Components"); // The 1st level nodes were sorted.
        root.SubItems[0].SubItems.Select(node => node.Caption).Is("Button", "Select"); // The 2nd level nodes were also sorted.
        root.SubItems[0].SubItems[0].SubItems.Select(node => node.Caption).Is("Docs", "Default", "Primary", "Danger"); // Stories were kept in the original order.
        root.SubItems[0].SubItems[1].SubItems.Select(node => node.Caption).Is("Docs", "Single Select", "Multiple Select");
    }
}
