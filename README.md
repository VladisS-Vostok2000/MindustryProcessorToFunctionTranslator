# MindustryProcessorToFunctionTranslator
Mindustry translator processing raw process file to function format.

# Overview
Some processor code may be pasted in several processors.
But when you using original mindustry ingame editor jumps 
you need to change its values as jumps usings static values.

This programm may translate default mindastry code (that i marked as `*.min` - mindustry) to
function code (marked `.minfun` - mindustry function) that you can paste in any part of new processor.
It translates static values (jump digits) to default processor language labels that are not static.

Let's say you have a function with jumps:
```
print "Function v1.0\n"
jump 4 notEqual variable null
print "variable null."
set @counter return

print "Do some actions..."
set @counter return
```
This function checks variable for a null value.
If value is null, functions prints "variable null." and returns.
If value not null, function prints "Do some actions..." and returns.

In presented code uses `jump 4` instruction.
Its bad, because if you paste your function in the middle or end of processor code
processor, it will break:

```
// Setting function location as its line number
// (comments and empty lines not counts there)
set BindCheckUnit 7
print "Processor v1.0"
print "Do some actions..."
// Function call
op add return @counter 1
set @counter BindCheckUnit

print "Do some actions..."
end


print "Function v1.0\n"
jump 4 notEqual variable null
print "variable null."
set @counter return

print "Do some actions..."
set @counter return
```

Now `jump 4` sends us not to `print "Do some actions..."` but `set @counter BindCheckUnit` that causes 
endless loop.

To fix that, you need manually set new value to jump in new processor.

In big functions you need to count a lot of jump's lines, that is absolutely unbearable.

To avoid this, you may use labels instead static values:
```
print "Function v1.0\n"
jump Label0 notEqual variable null
print "variable null."
set @counter return

Label0:
print "Do some actions..."
set @counter return
```

Now, when we pasted this function into code, it will work correctly.

If you using ingame editor, you cant use labels, so you placing jumps.
So you have a function code in raw format.

This programm will translate raw function code (with static `jump`'s values) to labels ones.
You just need to copy processor code to clipboard and paste it in file, and run the programm with specific arguments.

# Using

## No arguments
This programm can be run without arguments. This means, you can run it by `.exe` file.
In that case, all `.min` files in its folder will be translated to `.minraw` files. `.min` files will not be removed or changed.

## One parameter
If you need to translate specific file or files in folder, you may run your terminal and cast its argument after path to `.exe` file.
For example:
```
> "G:\Programs\MindustryProcessorToFunctionTranslator.exe" "C:\Mindustry\Processors"
```


