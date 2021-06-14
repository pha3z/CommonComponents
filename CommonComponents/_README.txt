Common Components is used by the ASP Rest bootstrap as well as Console Bootstrap solutions.

Common Components is meant to be globally pervasive across all projects meaning it should
contain reusable things that you are likely to find in most projects.

You should be able to safely leave it as a global reference in your project.  You shouldn't need to make a copy of it.
It shouldn't have things that are are likely to break other projects due to revisions.
It should only contain simple constructs that aren't likely to change.

The only thing that IS presently in FLUX is Result class, and this is pretty critical.
Its recommended that you copy the RESULT class into your application and depend on it there.
