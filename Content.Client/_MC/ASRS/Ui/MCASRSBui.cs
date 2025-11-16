using Content.Shared._MC.ASRS.Components;
using JetBrains.Annotations;
using Robust.Client.UserInterface;
using Robust.Shared.Prototypes;

namespace Content.Client._MC.ASRS.Ui;

[UsedImplicitly]
public sealed class MCASRSBui(EntityUid owner, Enum uiKey) : BoundUserInterface(owner, uiKey)
{
    [Dependency] private readonly IEntityManager _entity = default!;
    [Dependency] private readonly IPrototypeManager _prototypes = default!;

    [ViewVariables]
    private MCASRSWindow? _window;

    protected override void Open()
    {
        base.Open();

        _window = this.CreateWindow<MCASRSWindow>();


        if (!_entity.TryGetComponent<MCASRSConsoleComponent>(Owner, out var computer))
            return;

        foreach (var category in computer.Categories)
        {
            var categoryButton = new MCASRSCategoryButton();
            categoryButton.SetName(category.Name);
            categoryButton.Button.OnPressed += _ => LoadCategory(category);

            _window.CategoryView.CategoriesContainer.AddChild(categoryButton);
        }
    }

    private void LoadCategory(MCASRSCategory category)
    {
        if (_window is null)
            return;

        var view = _window.OrdersView;
        view.CategoryNameLabel.SetMessage(category.Name);
        view.Container.Children.Clear();

        foreach (var entry in category.Entries)
        {
            var categoryButton = new MCASRSOrderButton();
            categoryButton.OrderNameLabel.SetMessage($"{entry.Name ?? "Unknown"} ({entry.Cost})");

            view.Container.Children.Add(categoryButton);
        }
    }
}
