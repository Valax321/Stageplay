using MonoGame.Framework.Content.Pipeline.Builder;
using Radish;

var buildParams = ContentBuilderParams.Parse(args);

var builder = new StageplayBuilder();
builder.Run(buildParams);
