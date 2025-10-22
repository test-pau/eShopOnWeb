using Microsoft.Playwright.NUnit;
using Microsoft.Playwright;
using System.Text.RegularExpressions;

namespace Microsoft.eShopWeb.PlaywrightTests;

[TestFixture]
public class ThemeTests : PageTest
{
    private const string BaseUrl = "http://localhost:5000";
    
    [Test]
    public async Task HomePage_Loads_Successfully()
    {
        // Arrange & Act
        await Page.GotoAsync(BaseUrl);
        
        // Assert
        await Expect(Page).ToHaveTitleAsync(new Regex("Microsoft.eShopOnWeb"));
        await Expect(Page.Locator("img[alt='eShop On Web']")).ToBeVisibleAsync();
    }
    
    [Test]
    public async Task ThemeToggleButton_IsVisible()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);
        
        // Act
        var themeButton = Page.Locator("#theme-toggle");
        
        // Assert
        await Expect(themeButton).ToBeVisibleAsync();
    }
    
    [Test]
    public async Task ThemeToggle_CyclesThroughThemes()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);
        var themeButton = Page.Locator("#theme-toggle");
        
        // Get initial theme
        var initialText = await themeButton.TextContentAsync();
        
        // Act - Click to cycle to next theme
        await themeButton.ClickAsync();
        await Page.WaitForTimeoutAsync(500); // Wait for theme to apply
        var secondText = await themeButton.TextContentAsync();
        
        // Assert - Theme should have changed
        Assert.That(secondText, Is.Not.EqualTo(initialText));
        
        // Act - Click again to cycle to third theme
        await themeButton.ClickAsync();
        await Page.WaitForTimeoutAsync(500);
        var thirdText = await themeButton.TextContentAsync();
        
        // Assert - Theme should have changed again
        Assert.That(thirdText, Is.Not.EqualTo(secondText));
        
        // Act - Click once more to cycle back
        await themeButton.ClickAsync();
        await Page.WaitForTimeoutAsync(500);
        var finalText = await themeButton.TextContentAsync();
        
        // Assert - Should be back to initial theme
        Assert.That(finalText, Is.EqualTo(initialText));
    }
    
    [Test]
    public async Task DarkTheme_AppliesToPage()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);
        var themeButton = Page.Locator("#theme-toggle");
        
        // Click until we get to dark theme
        var maxClicks = 3;
        for (int i = 0; i < maxClicks; i++)
        {
            var buttonText = await themeButton.TextContentAsync();
            if (buttonText != null && buttonText.Contains("Dark") && !buttonText.Contains("Auto"))
            {
                break;
            }
            await themeButton.ClickAsync();
            await Page.WaitForTimeoutAsync(500);
        }
        
        // Assert - Page should have dark theme attribute
        var htmlElement = Page.Locator("html");
        var themeAttr = await htmlElement.GetAttributeAsync("data-theme");
        Assert.That(themeAttr, Is.EqualTo("dark"));
    }
    
    [Test]
    public async Task LightTheme_AppliesToPage()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);
        var themeButton = Page.Locator("#theme-toggle");
        
        // Click until we get to light theme
        var maxClicks = 3;
        for (int i = 0; i < maxClicks; i++)
        {
            var buttonText = await themeButton.TextContentAsync();
            if (buttonText != null && buttonText.Contains("Light") && buttonText.Contains("☀"))
            {
                break;
            }
            await themeButton.ClickAsync();
            await Page.WaitForTimeoutAsync(500);
        }
        
        // Assert - Page should not have dark theme attribute
        var htmlElement = Page.Locator("html");
        var themeAttr = await htmlElement.GetAttributeAsync("data-theme");
        Assert.That(themeAttr, Is.Null.Or.Empty);
    }
    
    [Test]
    public async Task ThemePreference_PersistsAcrossPageReload()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);
        var themeButton = Page.Locator("#theme-toggle");
        
        // Set to dark theme
        var maxClicks = 3;
        for (int i = 0; i < maxClicks; i++)
        {
            var buttonText = await themeButton.TextContentAsync();
            if (buttonText != null && buttonText.Contains("Dark") && !buttonText.Contains("Auto"))
            {
                break;
            }
            await themeButton.ClickAsync();
            await Page.WaitForTimeoutAsync(500);
        }
        
        var buttonTextBeforeReload = await themeButton.TextContentAsync();
        
        // Act - Reload page
        await Page.ReloadAsync();
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert - Theme should persist
        var buttonTextAfterReload = await themeButton.TextContentAsync();
        Assert.That(buttonTextAfterReload, Is.EqualTo(buttonTextBeforeReload));
        
        var htmlElement = Page.Locator("html");
        var themeAttr = await htmlElement.GetAttributeAsync("data-theme");
        Assert.That(themeAttr, Is.EqualTo("dark"));
    }
    
    [Test]
    public async Task Navigation_WorksWithDarkTheme()
    {
        // Arrange
        await Page.GotoAsync(BaseUrl);
        var themeButton = Page.Locator("#theme-toggle");
        
        // Set to dark theme
        var maxClicks = 3;
        for (int i = 0; i < maxClicks; i++)
        {
            var buttonText = await themeButton.TextContentAsync();
            if (buttonText != null && buttonText.Contains("Dark") && !buttonText.Contains("Auto"))
            {
                break;
            }
            await themeButton.ClickAsync();
            await Page.WaitForTimeoutAsync(500);
        }
        
        // Act - Navigate to different pages
        // Check if login link exists and click it
        var loginLink = Page.Locator("a:has-text('Login')").First;
        if (await loginLink.IsVisibleAsync())
        {
            await loginLink.ClickAsync();
            await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
            
            // Assert - Theme persists on login page
            var htmlElement = Page.Locator("html");
            var themeAttr = await htmlElement.GetAttributeAsync("data-theme");
            Assert.That(themeAttr, Is.EqualTo("dark"));
        }
        
        // Navigate back to home
        await Page.GotoAsync(BaseUrl);
        await Page.WaitForLoadStateAsync(LoadState.NetworkIdle);
        
        // Assert - Theme still persists
        var htmlElementHome = Page.Locator("html");
        var themeAttrHome = await htmlElementHome.GetAttributeAsync("data-theme");
        Assert.That(themeAttrHome, Is.EqualTo("dark"));
    }
}
