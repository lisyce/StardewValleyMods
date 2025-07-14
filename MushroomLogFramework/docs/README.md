# Mushroom Log Framework

Mushroom Log Framework allows content pack authors to edit some of the hardcoded mushroom log behavior.

## Overview

The Vanilla algorithm for determining the output of a mushroom log is a bit complicated, but here's the gist:
1. An empty list of possible outputs is created.
2. Each fully-grown nearby tree contributes one possible output to the list, depending on the type of tree it is.
3. 