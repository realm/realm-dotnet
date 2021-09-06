import slackifyMarkdown from "slackify-markdown";

const changelogRegex = /^## (?<currentVersion>[^ ]*) \((?<date>[^)]*)\)[^#]*(?<sections>.*)/gms;
const sectionsRegex = /### (?<sectionName>[^\n]*)(?<sectionContent>.+?(?=###|$))/gs;

export function getPayload(changelog: string, sdk: string, repoUrl: string): SlackPayload {
    changelogRegex.lastIndex = 0;
    sectionsRegex.lastIndex = 0;

    const changelogMatch = changelogRegex.exec(changelog);
    if (!changelogMatch || !changelogMatch.groups) {
        throw new Error(`Failed to match changelog: ${changelog}`);
    }

    const version = changelogMatch.groups["currentVersion"];
    const date = changelogMatch.groups["date"];

    const slackSections = new Array<{ title: string; text: string }>();

    let sectionMatch: RegExpExecArray | null;
    while ((sectionMatch = sectionsRegex.exec(changelogMatch.groups["sections"]))) {
        if (!sectionMatch.groups) {
            throw new Error(`Failed to match sections: ${changelogMatch.groups["sections"]}`);
        }

        slackSections.push({ title: sectionMatch.groups["sectionName"], text: sectionMatch.groups["sectionContent"] });
    }

    const releaseUrl = `${repoUrl}/releases/tag/${version}`;
    const slackPayload = new SlackPayload(`Realm ${sdk} ${version}`, `*${date}* | <${releaseUrl}|${sdk} SDK Release>`);
    for (const section of slackSections) {
        slackPayload.addSection(section.title, section.text);
    }

    return slackPayload;
}

class SlackPayload {
    username = "Realm CI";
    icon_emoji = ":realm_new:";
    blocks: Block[] = [];

    constructor(header: string, context: string) {
        this.blocks.push(new HeaderBlock(header));
        this.blocks.push(new ContextBlock(context));
        this.blocks.push(new DividerBlock());
    }

    addSection(title: string, text: string): void {
        this.blocks.push(new SectionBlock(`*${title}*`));
        this.blocks.push(new SectionBlock(slackifyMarkdown(text)));
    }
}

type Block = SectionBlock | DividerBlock | HeaderBlock | ContextBlock;

class SectionBlock {
    type = "section";
    text = {
        type: "mrkdwn",
        text: "",
        verbatim: true,
    };

    constructor(text: string) {
        this.text.text = text;
    }
}

class HeaderBlock {
    type = "header";
    text = {
        type: "plain_text",
        text: "",
    };

    constructor(text: string) {
        this.text.text = text;
    }
}

class ContextBlock {
    type = "context";
    elements = [
        {
            text: "",
            type: "mrkdwn",
        },
    ];

    constructor(text: string) {
        this.elements[0].text = text;
    }
}

class DividerBlock {
    type = "divider";
}
