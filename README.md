# OpenToolkit (4.0) port of Ogldev's tutorial 38 on Skeletal Animations

You can find the original tutorial and code here: http://www.ogldev.org/www/tutorial38/tutorial38.html

This project attempts to be an (almost) line-by-line port of Ogldev's tutorial on skeletal animation. However, we are only concerned with the portions of the tutorial that has to do with skeletal animation. I did not bother porting the lighting or texturing of the model, among other things.

The purpose of this project is to be a reference implementation you can use while you follow along with Ogldev's tutorial, to iron out some of the gotchas for you. In particular you should take a look at the order in which matrixes are multiplied, when they are converted to OpenToolkit matrixes, and how.

You can ignore everything in the Utils namespace, except for the ToOtk() functions in Util.cs. Especially the method for converting matrixes may be different from what you would expect! FpsCamera, MathUtils and ShaderUtils were not ported from Ogldev's tutorial, they are there for convenience sake, so that I didn't have to port his entire project.

When you run the program you should see an animated model. You can also move around the scene with WASD, space and shift.

![](https://i.imgur.com/XYZ49eJ.gif)
