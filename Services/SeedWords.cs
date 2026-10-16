using EnglishStudy.Models;

namespace EnglishStudy.Services;

/// <summary>Banco de palavras e frases que acompanha o app (conteúdo embutido).</summary>
public static class SeedWords
{
    public const int Version = 1;

    public static List<WordEntry> All()
    {
        var list = new List<WordEntry>();
        var n = 0;

        void W(string pt, string en, string exEn, string exPt, string cat) =>
            list.Add(new WordEntry
            {
                Id = $"w{++n:000}",
                Pt = pt,
                En = en,
                ExampleEn = exEn,
                ExamplePt = exPt,
                Category = cat,
            });

        // ── Cumprimentos ────────────────────────────────────────────────
        W("oi, olá", "hello", "Hello! How are you today?", "Olá! Como você está hoje?", "greetings");
        W("bom dia", "good morning", "Good morning! Did you sleep well?", "Bom dia! Você dormiu bem?", "greetings");
        W("boa tarde", "good afternoon", "Good afternoon, everyone!", "Boa tarde a todos!", "greetings");
        W("boa noite", "good night", "Good night! See you tomorrow.", "Boa noite! Até amanhã.", "greetings");
        W("tchau", "goodbye", "Goodbye! Have a nice day.", "Tchau! Tenha um bom dia.", "greetings");
        W("por favor", "please", "Can you help me, please?", "Você pode me ajudar, por favor?", "greetings");
        W("obrigado(a)", "thank you", "Thank you for your help!", "Obrigado pela sua ajuda!", "greetings");
        W("de nada", "you're welcome", "You're welcome! It was easy.", "De nada! Foi fácil.", "greetings");
        W("desculpe", "sorry", "Sorry, I am late.", "Desculpe, estou atrasado.", "greetings");
        W("meu nome é", "my name is", "My name is Pedro.", "Meu nome é Pedro.", "greetings");
        W("prazer em conhecer você", "nice to meet you", "Nice to meet you! I am Ana.", "Prazer em conhecer você! Eu sou Ana.", "greetings");
        W("como você está", "how are you", "How are you? I am fine, thanks.", "Como você está? Estou bem, obrigado.", "greetings");
        W("até mais tarde", "see you later", "See you later, my friend!", "Até mais tarde, meu amigo!", "greetings");
        W("e aí?", "what's up", "What's up? Everything good?", "E aí? Tudo bem?", "greetings");

        // ── Rotina do Dia a Dia ─────────────────────────────────────────
        W("acordar", "wake up", "I wake up at seven every day.", "Eu acordo às sete todos os dias.", "routine");
        W("levantar", "get up", "I get up right after the alarm.", "Eu levanto logo depois do despertador.", "routine");
        W("escovar os dentes", "brush my teeth", "I brush my teeth after breakfast.", "Eu escovo os dentes depois do café.", "routine");
        W("tomar banho", "take a shower", "She takes a shower in the morning.", "Ela toma banho de manhã.", "routine");
        W("tomar café da manhã", "have breakfast", "We have breakfast together at eight.", "Nós tomamos café da manhã juntos às oito.", "routine");
        W("ir ao trabalho", "go to work", "I go to work by bus.", "Eu vou ao trabalho de ônibus.", "routine");
        W("almoçar", "have lunch", "Where do you have lunch?", "Onde você almoça?", "routine");
        W("jantar", "have dinner", "We have dinner at home every night.", "Nós jantamos em casa toda noite.", "routine");
        W("estudar", "study", "I study English every night.", "Eu estudo inglês toda noite.", "routine");
        W("dormir", "sleep", "I sleep eight hours every night.", "Eu durmo oito horas toda noite.", "routine");
        W("sempre", "always", "She is always happy.", "Ela está sempre feliz.", "routine");
        W("às vezes", "sometimes", "Sometimes I watch TV at night.", "Às vezes eu assisto TV à noite.", "routine");
        W("nunca", "never", "He never drinks soda.", "Ele nunca bebe refrigerante.", "routine");
        W("hoje", "today", "Today is a beautiful day.", "Hoje é um dia bonito.", "routine");
        W("amanhã", "tomorrow", "See you tomorrow morning.", "Até amanhã de manhã.", "routine");
        W("fim de semana", "weekend", "I relax on the weekend.", "Eu relaxo no fim de semana.", "routine");

        // ── Comidas e Bebidas ───────────────────────────────────────────
        W("água", "water", "Can I have a glass of water, please?", "Pode me trazer um copo de água, por favor?", "food");
        W("café", "coffee", "I drink coffee every morning.", "Eu bebo café toda manhã.", "food");
        W("pão", "bread", "I eat bread with cheese.", "Eu como pão com queijo.", "food");
        W("queijo", "cheese", "This pizza has a lot of cheese.", "Esta pizza tem muito queijo.", "food");
        W("arroz e feijão", "rice and beans", "Rice and beans is a typical Brazilian meal.", "Arroz e feijão é uma refeição típica brasileira.", "food");
        W("frango", "chicken", "I like grilled chicken.", "Eu gosto de frango grelhado.", "food");
        W("peixe", "fish", "We eat fish on Fridays.", "Nós comemos peixe às sextas.", "food");
        W("fruta", "fruit", "Fruit is good for your health.", "Fruta faz bem para a saúde.", "food");
        W("maçã", "apple", "An apple a day keeps the doctor away.", "Uma maçã por dia mantém o médico longe.", "food");
        W("leite", "milk", "I drink milk before bed.", "Eu bebo leite antes de dormir.", "food");
        W("suco", "juice", "Orange juice, please.", "Suco de laranja, por favor.", "food");
        W("com fome", "hungry", "I am hungry. Let's eat something!", "Estou com fome. Vamos comer algo!", "food");
        W("com sede", "thirsty", "I am thirsty. Can I have some water?", "Estou com sede. Pode me dar água?", "food");
        W("delicioso(a)", "delicious", "This food is delicious!", "Esta comida está deliciosa!", "food");
        W("pedir", "order", "I want to order a hamburger.", "Eu quero pedir um hambúrguer.", "food");
        W("restaurante", "restaurant", "The restaurant is open at noon.", "O restaurante abre ao meio-dia.", "food");
        W("a conta", "bill", "Can I have the bill, please?", "Pode trazer a conta, por favor?", "food");

        // ── Casa e Família ──────────────────────────────────────────────
        W("casa", "house", "My house is small but cozy.", "Minha casa é pequena, mas aconchegante.", "home");
        W("lar, em casa", "home", "I am at home right now.", "Eu estou em casa agora.", "home");
        W("família", "family", "I love my family.", "Eu amo minha família.", "home");
        W("mãe", "mother", "My mother cooks very well.", "Minha mãe cozinha muito bem.", "home");
        W("pai", "father", "My father works a lot.", "Meu pai trabalha muito.", "home");
        W("irmão", "brother", "My brother is older than me.", "Meu irmão é mais velho que eu.", "home");
        W("irmã", "sister", "My sister lives in Sao Paulo.", "Minha irmã mora em São Paulo.", "home");
        W("amigo(a)", "friend", "He is my best friend.", "Ele é meu melhor amigo.", "home");
        W("cachorro", "dog", "My dog sleeps in my bed.", "Meu cachorro dorme na minha cama.", "home");
        W("gato", "cat", "The cat is on the sofa.", "O gato está no sofá.", "home");
        W("quarto", "bedroom", "My bedroom is quiet.", "Meu quarto é silencioso.", "home");
        W("cozinha", "kitchen", "The kitchen is clean.", "A cozinha está limpa.", "home");
        W("banheiro", "bathroom", "Where is the bathroom?", "Onde fica o banheiro?", "home");
        W("vizinho(a)", "neighbor", "My neighbor is very friendly.", "Meu vizinho é muito simpático.", "home");
        W("morar", "live", "I live in a big city.", "Eu moro em uma cidade grande.", "home");

        // ── Viagem ──────────────────────────────────────────────────────
        W("aeroporto", "airport", "The airport is far from the city.", "O aeroporto é longe da cidade.", "travel");
        W("voo", "flight", "My flight leaves at six in the morning.", "Meu voo sai às seis da manhã.", "travel");
        W("passagem, bilhete", "ticket", "I need to buy a ticket to Rio.", "Eu preciso comprar uma passagem para o Rio.", "travel");
        W("passaporte", "passport", "Do not forget your passport!", "Não esqueça seu passaporte!", "travel");
        W("hotel", "hotel", "The hotel has a swimming pool.", "O hotel tem piscina.", "travel");
        W("bagagem", "luggage", "My luggage is heavy.", "Minha bagagem está pesada.", "travel");
        W("ônibus", "bus", "The bus stops here every hour.", "O ônibus para aqui a cada hora.", "travel");
        W("táxi", "taxi", "Let's take a taxi to the airport.", "Vamos pegar um táxi até o aeroporto.", "travel");
        W("praia", "beach", "I go to the beach in the summer.", "Eu vou à praia no verão.", "travel");
        W("mapa", "map", "Can you show me on the map?", "Você pode me mostrar no mapa?", "travel");
        W("chegar", "arrive", "The train arrives at nine.", "O trem chega às nove.", "travel");
        W("partir, sair", "leave", "The train leaves at midnight.", "O trem parte à meia-noite.", "travel");
        W("turista", "tourist", "The tourist speaks Portuguese.", "O turista fala português.", "travel");
        W("viagem", "trip", "The trip was amazing!", "A viagem foi incrível!", "travel");
        W("perto", "near", "Is the station near here?", "A estação é perto daqui?", "travel");
        W("longe", "far", "The museum is far from here.", "O museu é longe daqui.", "travel");

        // ── Trabalho e Estudo ───────────────────────────────────────────
        W("emprego", "job", "She has a new job.", "Ela tem um emprego novo.", "work");
        W("reunião", "meeting", "The meeting starts at ten.", "A reunião começa às dez.", "work");
        W("escritório", "office", "I work in an office downtown.", "Eu trabalho em um escritório no centro.", "work");
        W("chefe", "boss", "My boss is very kind.", "Meu chefe é muito gentil.", "work");
        W("e-mail", "email", "I send an email to my boss.", "Eu envio um e-mail para meu chefe.", "work");
        W("professor(a)", "teacher", "The teacher explains the lesson.", "O professor explica a lição.", "work");
        W("estudante", "student", "I am a student at the university.", "Eu sou estudante na universidade.", "work");
        W("dever de casa", "homework", "I do my homework at night.", "Eu faço meu dever de casa à noite.", "work");
        W("livro", "book", "This book is interesting.", "Este livro é interessante.", "work");
        W("computador", "computer", "My computer is new.", "Meu computador é novo.", "work");
        W("aprender", "learn", "I want to learn English fast.", "Eu quero aprender inglês rápido.", "work");
        W("praticar", "practice", "Practice makes perfect.", "A prática leva à perfeição.", "work");
        W("escrever", "write", "I write notes in my notebook.", "Eu escrevo anotações no meu caderno.", "work");
        W("falar", "speak", "Do you speak English?", "Você fala inglês?", "work");
        W("dinheiro", "money", "I need to save money.", "Eu preciso economizar dinheiro.", "work");

        // ── Frases Úteis ────────────────────────────────────────────────
        W("eu não entendo", "I don't understand", "Sorry, I don't understand. Can you repeat?", "Desculpe, eu não entendo. Pode repetir?", "conversation");
        W("você pode repetir, por favor?", "Can you repeat, please", "Can you repeat, please? Slower, please.", "Você pode repetir, por favor? Mais devagar.", "conversation");
        W("ajudar", "help", "Can you help me, please?", "Você pode me ajudar, por favor?", "conversation");
        W("eu gosto disso", "I like it", "I like it very much!", "Eu gosto disso muito!", "conversation");
        W("claro, com certeza", "of course", "Of course I can help you.", "Claro que posso te ajudar.", "conversation");
        W("talvez", "maybe", "Maybe we can meet tomorrow.", "Talvez a gente possa se encontrar amanhã.", "conversation");
        W("devagar", "slowly", "Please speak slowly.", "Por favor, fale devagar.", "conversation");
        W("eu estou aprendendo", "I'm learning", "I'm learning English every day.", "Eu estou aprendendo inglês todo dia.", "conversation");
        W("com licença", "excuse me", "Excuse me, where is the station?", "Com licença, onde fica a estação?", "conversation");
        W("sem problema", "no problem", "No problem! I can wait.", "Sem problema! Eu posso esperar.", "conversation");
        W("eu concordo", "I agree", "I agree with you.", "Eu concordo com você.", "conversation");
        W("um momento", "one moment", "One moment, please.", "Um momento, por favor.", "conversation");

        return list;
    }
}
