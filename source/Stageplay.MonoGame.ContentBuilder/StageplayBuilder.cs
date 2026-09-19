using MonoGame.Framework.Content.Pipeline.Builder;

namespace Radish;

public class StageplayBuilder : ContentBuilder
{
    public override IContentCollection GetContentCollection()
    {
        var content = new ContentCollection();
        content.SetContentRoot(string.Empty);
        
        return content;
    }
}