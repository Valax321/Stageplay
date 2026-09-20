namespace Radish.ContentBuilder;

public interface ICookRecipe
{
    BuilderTask Task { get; init; }

    void BuildProcessorQueue(ProcessorQueue queue);
}