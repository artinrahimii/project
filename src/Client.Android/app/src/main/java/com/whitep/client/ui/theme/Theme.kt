package com.whitep.client.ui.theme

import androidx.compose.material3.MaterialTheme
import androidx.compose.material3.lightColorScheme
import androidx.compose.runtime.Composable

/** Shared light theme. Palette values are generated from the repository token source. */
@Composable
internal fun whitePTheme(content: @Composable () -> Unit) {
    val ink = WhitePBrandColors.midnightInk
    val paper = WhitePBrandColors.porcelain
    val white = WhitePBrandColors.pureWhite
    val gold = WhitePBrandColors.antiqueGold
    val teal = WhitePBrandColors.mutedTeal

    val colors = lightColorScheme(
        primary = ink,
        onPrimary = white,
        primaryContainer = paper,
        onPrimaryContainer = ink,
        inversePrimary = gold,
        secondary = teal,
        onSecondary = white,
        secondaryContainer = paper,
        onSecondaryContainer = ink,
        tertiary = gold,
        onTertiary = ink,
        tertiaryContainer = paper,
        onTertiaryContainer = ink,
        background = paper,
        onBackground = ink,
        surface = white,
        onSurface = ink,
        surfaceVariant = paper,
        onSurfaceVariant = ink,
        surfaceTint = ink,
        inverseSurface = ink,
        inverseOnSurface = white,
        error = ink,
        onError = white,
        errorContainer = paper,
        onErrorContainer = ink,
        outline = teal,
        outlineVariant = teal,
        scrim = ink,
        surfaceDim = paper,
        surfaceBright = white,
        surfaceContainerLowest = white,
        surfaceContainerLow = paper,
        surfaceContainer = white,
        surfaceContainerHigh = paper,
        surfaceContainerHighest = white,
    )
    MaterialTheme(
        colorScheme = colors,
        content = content,
    )
}
