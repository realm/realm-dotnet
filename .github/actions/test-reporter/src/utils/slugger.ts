// Returns HTML element id and href link usable as manual anchor links
// This is needed because Github in check run summary doesn't automatically
// create links out of headings as it normally does for other markdown content
export function slug(name: string): {id: string; link: string} {
  const slugId = name
    .trim()
    .replace(/_/g, '')
    .replace(/[./\\]/g, '-')
    .replace(/[^\w-]/g, '')

  const id = `user-content-${slugId}`
  const link = `#${slugId}`
  return {id, link}
}
