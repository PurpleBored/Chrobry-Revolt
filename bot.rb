require 'discordrb'
require 'open-uri'
require 'json'
require 'dotenv/load'

TOKEN = ENV['BOT_TOKEN']
bot = Discordrb::Bot.new(token: TOKEN)

# Help command
bot.message(content: '!help') do |event|
  embed = Discordrb::Webhooks::Embed.new(
    title: 'Available Categories',
    description: 'To get help for a specific category, use `!help-category_name`. (everything needs to be in lower case.)',
    color: 800080
  )
  embed.add_field(name: 'help-fun', value: 'Fun commands')
  embed.add_field(name: 'help-mod', value: 'Mod commands')
  # Add more categories as needed

  event.channel.send_embed('', embed)
end

# Fun help command
bot.message(content:('!help-fun')) do |event|
  # Provide help for fun commands in this category
  event.respond("Here are the fun commands: !joke, !sploot, !roast, !dogfact, !rps, !shitpost")
end

# Mod help command
bot.message(content:('!help-mod')) do |event|
  # Provide help for mod commands in this category
  event.respond("Here are the mod commands: !kick @user, !purge <num>")
end
# End of help command


# Simple joke command.
bot.message(content: '!joke') do |event|
  begin
    response = JSON.parse(open('https://v2.jokeapi.dev/joke/Any?type=twopart').read)
    setup = response['setup']
    punchline = response['delivery']
    event.respond(setup)
    event.respond(punchline)
  rescue StandardError => e
    event.respond("Sorry, I couldn't fetch a joke at the moment. Please try again later.")
    puts "Error fetching joke: #{e.message}"
  end
end
# End of Joke Command.

# Sploot
bot.message(content: '!sploot') do |event|
  # You can replace this URL with any other source of sploot images.
  sploot_image_url = 'https://cdn.discordapp.com/attachments/1030188681472000120/1152930346229301338/YUVfoERh3CiqXUdc9a9qwZ9paHWPf4GETsIslDBExFqsRVGds9pk9MicwErTq1N8sowFpUyO5Q9oVgs640-nd-v1.png'

  event.respond(sploot_image_url)
end
# End of Sploot Command

# Roast command.
bot.message(content: '!roast') do |event|
  roasts = [
    "You must be a parking ticket, because you've got 'Fine' written all over you.",
    "If you were any more inbred, you'd be a sandwich.",
    "I'd say you're dumb as a rock, but at least a rock can serve a purpose.",
    "Is your ass jealous of the amount of shit that comes out of your mouth?",
    "You're not pretty enough to be this stupid."
  ]
  event.respond(roasts.sample)
end

# Kick Command
bot.message(content: /^!kick\s<@(\d+)>$/) do |event|
  user_id = event.content.match(/^!kick\s<@(\d+)>$/)[1].to_i

  # Check if the user has the necessary permissions to kick members
  if event.author.permission?(:kick_members)
    begin
      user_to_kick = event.server.member(user_id)
      user_to_kick.kick
      event.respond("Successfully kicked <@#{user_id}>.")
    rescue StandardError => e
      event.respond("Error: #{e.message}")
    end
  else
    event.respond("You do not have permission to kick members.")
  end
end
# End of the kick command

# Ping Command
bot.message(content: '!ping') do |event|
  start_time = Time.now
  msg = event.respond('Pong!')
  end_time = Time.now

  ping = ((end_time - start_time) * 1000).to_i
  msg.edit("Pong! Latency is #{ping} ms.")
end
# End of ping command.

# Purge Command.
bot.message(content: /^!purge (\d{1,2})$/) do |event|
  num_to_delete = event.message.content.match(/^!purge (\d{1,2})$/)[1].to_i

  if num_to_delete >= 1 && num_to_delete <= 100
    event.channel.prune(num_to_delete + 1) # Adding 1 to also delete the command message
  else
    event.respond("Please specify a number between 1 and 100.")
  end
end
# End of Purge Command.

# Dogfact Command.
def fetch_dog_fact
  url = URI.parse('https://dog-api.kinduff.com/api/facts')
  response = Net::HTTP.get(url)
  JSON.parse(response)['facts'].first
end

bot.message(content: /^!dogfact$/) do |event|
  dog_fact = fetch_dog_fact
  event.respond("#{dog_fact}")
end
# End of Dogfact

# RPS Command.
valid_choices = ['rock', 'paper', 'scissors']

bot.message(content: /^!rps (rock|paper|scissors)$/) do |event|
  user_choice = event.message.content.match(/^!rps (rock|paper|scissors)$/)[1]

  if valid_choices.include?(user_choice)
    bot_choice = valid_choices.sample

    result = case [user_choice, bot_choice]
             when ['rock', 'scissors'], ['paper', 'rock'], ['scissors', 'paper']
               "You win!"
             when ['scissors', 'rock'], ['rock', 'paper'], ['paper', 'scissors']
               "I win!"
             else
               "It's a tie!"
             end

    event.respond("You chose #{user_choice}, I chose #{bot_choice}. #{result}")
  else
    event.respond("Please choose either rock, paper, or scissors!")
  end
end
# End of RPS command

# Begining of the Shitpost command.
bot.message(content: '!shitpost') do |event|
  begin
    uri = URI.parse('https://api.thedailyshitpost.net/random')
    response = Net::HTTP.get_response(uri)

    if response.is_a?(Net::HTTPSuccess)
      meme_json = JSON.parse(response.body)
      title = meme_json['title']
      image_url = meme_json['url']
      event.respond("#{title}: #{image_url}")
    else
      event.respond("Sorry, I couldn't fetch a meme at the moment. 99% chance that ratelimit or Shitpost API dead.")
    end
  rescue StandardError => e
    event.respond("An error occurred: #{e.message}")
  end
end
# End of the Shitpost command.

bot.run

